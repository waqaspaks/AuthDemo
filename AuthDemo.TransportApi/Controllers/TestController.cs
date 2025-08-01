using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthDemo.TransportApi.Controllers
{
    /// <summary>
    /// Test API endpoint demonstrating manual scope-based authorization.
    /// 
    /// Unlike other controllers that use policies, this controller:
    /// - Validates authentication using <see cref="OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme"/>.
    /// - Manually checks for a specific scope (<c>manager.transport.api</c>) in the user's claims.
    /// - Returns <c>403 Forbidden</c> with appropriate error details if the scope is missing.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class TestController : ControllerBase
    {
        /// <summary>
        /// GET endpoint that validates the presence of the "manager.transport.api" scope.
        /// </summary>
        /// <remarks>
        /// - This endpoint does not use a policy; it performs manual scope validation.
        /// - If the required scope is missing, the request is denied with a <c>403 Forbidden</c> response,
        ///   along with error details in the response properties.
        /// </remarks>
        /// <returns>
        /// <para>
        /// <c>200 OK</c> if the user has the <c>manager.transport.api</c> scope.
        /// </para>
        /// <para>
        /// <c>403 Forbidden</c> if the scope is missing, with a descriptive error.
        /// </para>
        /// </returns>
        [HttpGet]
        public IActionResult Get()
        {
            // Manually check if the authenticated user has the required scope.
            if (!User.HasScope("manager.transport.api"))
            {
                // Return a forbidden response with error details for OpenIddict to handle.
                return Forbid(
                    authenticationSchemes: OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictValidationAspNetCoreConstants.Properties.Scope] = "manager.transport.api",
                        [OpenIddictValidationAspNetCoreConstants.Properties.Error] = Errors.InsufficientScope,
                        [OpenIddictValidationAspNetCoreConstants.Properties.ErrorDescription] =
                            "The 'manager.transport.api' scope is required to perform this action."
                    }));
            }

            // If the user has the required scope, return a success message.
            return Ok(new
            {
                message = "Access Granted because the request has the 'manager.transport.api' scope."
            });
        }
    }
}
