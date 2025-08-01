using System.ComponentModel.DataAnnotations;

namespace AuthDemo.Shared.Models
{
    /// <summary>
    /// Represents the login request model containing user credentials.
    /// </summary>
    public class LoginModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = "";
    }

    /// <summary>
    /// Represents the response returned after a login attempt.
    /// </summary>
    public class LoginResponse
    {
        public bool Succeeded { get; set; }
        public string? Message { get; set; }
    }
}