using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthDemo.Controllers;
/// <summary>
/// 
/// </summary>
public class AuthorizationController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthorizationController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    [HttpPost("~/connect/token")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return BadRequest();
        }

        // Handle password grant type
        if (request.IsPasswordGrantType())
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }
            var roles = await _userManager.GetRolesAsync(user);
            var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                Claims.Name, Claims.Role);

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

            foreach (var role in roles)
            {
                var roleClaim = new Claim(Claims.Role, role);
                roleClaim.SetDestinations(Destinations.AccessToken, Destinations.IdentityToken);
                identity.AddClaim(roleClaim);
            }

            var principal = new ClaimsPrincipal(identity);

            var scopes = roles.SelectMany(r => r switch
            {
                "Admin" => new[] { "admin.demo.api", "manager.demo.api", "admin.mock.api", "manager.mock.api", "user.demo.api" },
                "Manager" => new[] { "manager.demo.api", "manager.mock.api", "user.demo.api" },
                _ => new[] { "user.demo.api" }
            }).Distinct().ToList();

            if (request.GetScopes().Contains("offline_access"))
                scopes.Add("offline_access");


            principal.SetScopes(scopes);

            //// If using only principal.SetScopes(scopes) does not work,
            //// try combining the scopes into a single space-separated string using the lines below:
            //var scopeString = string.Join(' ', scopes);
            //principal.AddClaim(Claims.Scope, scopeString, Destinations.AccessToken);
            var audiences = scopes.Contains("manager.demo.api") ? new[] { "DemoApi", "MockApi" } : new[] { "DemoApi" };
            principal.SetAudiences(audiences);

            principal.SetDestinations(claim =>
            claim.Type switch
            {
                Claims.Scope => new[] { Destinations.AccessToken },
                Claims.Audience => new[] { Destinations.AccessToken },
                Claims.Role => new[] { Destinations.AccessToken },
                Claims.Issuer => new[] { Destinations.AccessToken },
                _ => Array.Empty<string>()
            });

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (result?.Principal == null)
            {
                return Forbid();
            }
            return SignIn(result.Principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotImplementedException("The specified grant type is not implemented.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request == null)
        {
            return BadRequest();
        }

        var result = await HttpContext.AuthenticateAsync();
        if (result == null || !result.Succeeded || result.Principal == null)
        {
            return Challenge(properties: new AuthenticationProperties
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

        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            Claims.Name,
            Claims.Role);

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

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            identity.AddClaim(Claims.Role, role);
        }

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(request.GetScopes());

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("~/connect/userinfo")]
    public async Task<IActionResult> Userinfo()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var email = await _userManager.GetEmailAsync(user);
        var userName = await _userManager.GetUserNameAsync(user);

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