using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthDemo.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
public class TestController : ControllerBase
{
    [HttpGet]
    //[Authorize(Policy = "ManagerScope")]
    public IActionResult Get()
    {
        if(!User.HasScope("manager.api"))
        {
            return Forbid(
                           authenticationSchemes: OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme,
                           properties: new AuthenticationProperties(new Dictionary<string, string>
                           {
                               [OpenIddictValidationAspNetCoreConstants.Properties.Scope] = "manager.api",
                               [OpenIddictValidationAspNetCoreConstants.Properties.Error] = Errors.InsufficientScope,
                               [OpenIddictValidationAspNetCoreConstants.Properties.ErrorDescription] =
                                   "The 'manager.api' scope is required to perform this action."
                           }));
        }
        return Ok(new { message = "Access Granted because the request has the 'manager.api' scope " });
    }
}