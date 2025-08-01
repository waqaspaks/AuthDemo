using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthDemo.Controllers
{
    /// <summary>
    /// Controller responsible for handling authentication, authorization,
    /// token issuance, refresh tokens, and user information using OpenIddict.
    /// </summary>
    public class AuthorizationController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationController"/>.
        /// </summary>
        /// <param name="signInManager">Manages sign-in operations for users.</param>
        /// <param name="userManager">Provides user management operations like finding users, validating passwords, and retrieving roles.</param>
        public AuthorizationController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        /// <summary>
        /// Handles token requests (Password Grant and Refresh Token Grant).
        /// </summary>
        /// <returns>A signed-in result with an access token, or an error if authentication fails.</returns>
        [HttpPost("~/connect/token")]
        public async Task<IActionResult> Exchange()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
            {
                return BadRequest();
            }

            // Handle Resource Owner Password Credentials (Password Grant)
            if (request.IsPasswordGrantType())
            {
                // Validate user credentials
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    // Invalid username or password
                    return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                }

                // Retrieve user roles
                var roles = await _userManager.GetRolesAsync(user);

                // Create a new claims identity for the user
                var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    Claims.Name, Claims.Role);

                // Add user claims (Subject, Email, Name)
                var userId = await _userManager.GetUserIdAsync(user);
                var email = await _userManager.GetEmailAsync(user);
                var userName = await _userManager.GetUserNameAsync(user);

                var subjectClaim = new Claim(Claims.Subject, userId);
                subjectClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
                identity.AddClaim(subjectClaim);

                if (email != null)
                {
                    var emailClaim = new Claim(Claims.Email, email);
                    emailClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
                    identity.AddClaim(emailClaim);
                }

                if (userName != null)
                {
                    var nameClaim = new Claim(Claims.Name, userName);
                    nameClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
                    identity.AddClaim(nameClaim);
                }

                // Add user role claims
                foreach (var role in roles)
                {
                    var roleClaim = new Claim(Claims.Role, role);
                    roleClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
                    identity.AddClaim(roleClaim);
                }

                var principal = new ClaimsPrincipal(identity);

                // Assign scopes based on user roles
                var scopes = roles.SelectMany(r => r switch
                {
                    "Admin" => new[] { "admin.transport.api", "manager.transport.api", "admin.sports.api", "manager.sports.api", "user.transport.api" },
                    "Manager" => new[] { "manager.transport.api", "manager.sports.api", "user.transport.api" },
                    _ => new[] { "user.transport.api" }
                }).Distinct().ToList();

                // Add offline_access if requested
                if (request.GetScopes().Contains("offline_access"))
                    scopes.Add("offline_access");

                principal.SetScopes(scopes);

                // Set API audiences based on scopes
                var audiences = scopes.Contains("manager.transport.api")
                    ? new[] { "TransportApi", "SportsApi" }
                    : new[] { "TransportApi" };

                principal.SetAudiences(audiences);

                // Set destinations for claims
                principal.SetDestinations(claim =>
                    claim.Type switch
                    {
                        Claims.Scope => new[] { Destinations.AccessToken },
                        Claims.Audience => new[] { Destinations.AccessToken },
                        Claims.Role => new[] { Destinations.AccessToken },
                        Claims.Issuer => new[] { Destinations.AccessToken },
                        _ => Array.Empty<string>()
                    });

                // Return the token response
                return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Handle Refresh Token Grant
            if (request.IsRefreshTokenGrantType())
            {
                var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
                if (result?.Principal == null)
                {
                    return Forbid();
                }

                return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            // Unsupported grant type
            throw new NotImplementedException("The specified grant type is not implemented.");
        }

        /// <summary>
        /// Handles interactive authorization requests (Authorization Code/Implicit Flow).
        /// </summary>
        /// <returns>A sign-in result with an authorization code or token.</returns>
        [HttpGet("~/connect/authorize")]
        [HttpPost("~/connect/authorize")]
        public async Task<IActionResult> Authorize()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
            {
                return BadRequest();
            }

            // Authenticate the current user
            var result = await HttpContext.AuthenticateAsync();
            if (result == null || !result.Succeeded || result.Principal == null)
            {
                // Redirect to login page if the user is not authenticated
                return Challenge(new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
            }

            var user = await _userManager.GetUserAsync(result.Principal);
            if (user == null)
            {
                return Forbid();
            }

            // Create a new claims identity
            var identity = new ClaimsIdentity(
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                Claims.Name,
                Claims.Role);

            // Add claims
            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var userName = await _userManager.GetUserNameAsync(user);

            identity.AddClaim(Claims.Subject, userId);
            if (email != null)
            {
                identity.AddClaim(Claims.Email, email);
            }
            if (userName != null)
            {
                identity.AddClaim(Claims.Name, userName);
            }

            // Add roles
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                identity.AddClaim(Claims.Role, role);
            }

            var principal = new ClaimsPrincipal(identity);

            // Set the requested scopes
            principal.SetScopes(request.GetScopes());

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Returns information about the currently authenticated user.
        /// </summary>
        /// <returns>A JSON object containing user details such as subject, email, and name.</returns>
        [HttpGet("~/connect/userinfo")]
        public async Task<IActionResult> Userinfo()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Challenge if user is not authenticated
                return Challenge(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var userName = await _userManager.GetUserNameAsync(user);

            // Return user claims
            var claims = new Dictionary<string, object>
            {
                [Claims.Subject] = userId
            };

            if (email != null)
            {
                claims[Claims.Email] = email;
            }
            if (userName != null)
            {
                claims[Claims.Name] = userName;
            }

            return Ok(claims);
        }
    }
}
