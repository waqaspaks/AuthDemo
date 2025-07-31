using System.ComponentModel.DataAnnotations;
using AuthDemo.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthDemo.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="request">The registration request containing email and password</param>
    /// <returns>A response indicating success or failure with error messages</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterModel request)
    {
        // Model validation is automatically handled by [ApiController]
        var response = new RegisterResponse();

        // Check if user already exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            response.Succeeded = false;
            response.Message = "User with this email already exists";
            return BadRequest(response);
        }

        var user = new IdentityUser { UserName = request.Email, Email = request.Email };
        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {// Add role to the user after registration
            var roleResult = await _userManager.AddToRoleAsync(user, request.Role ?? "User");
            if (!roleResult.Succeeded)
            {
                response.Succeeded = false;
                response.Message = "User created but failed to assign role";
                response.Errors = roleResult.Errors.Select(e => e.Description).ToList();
                return BadRequest(response);
            }
            response.Succeeded = true;
            response.Message = "Registration successful";
            return Ok(response);
        }

        response.Succeeded = false;
        response.Message = "Registration failed";
        response.Errors = result.Errors.Select(e => e.Description).ToList();
        return BadRequest(response);
    }
}