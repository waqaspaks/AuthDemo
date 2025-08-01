using AuthDemo.Sports.Models;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using AuthDemo.Shared.Models;

namespace AuthDemo.Sports.Services
{
    /// <summary>
    /// Service responsible for handling user registration by communicating with the Identity API.
    /// </summary>
    public class RegisterService
    {
        private readonly ApiService _apiService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterService"/> class.
        /// </summary>
        /// <param name="apiService">Service used to make API calls.</param>
        public RegisterService(ApiService apiService)
        {
            _apiService = apiService;
        }

        /// <summary>
        /// Handles user registration by sending registration data to the Identity API.
        /// </summary>
        /// <param name="model">The registration model containing user details.</param>
        /// <returns>
        /// A tuple containing:
        /// <list type="bullet">
        /// <item><c>successMessage</c> if registration was successful.</item>
        /// <item><c>errorMessage</c> if registration failed.</item>
        /// </list>
        /// </returns>
        public async Task<(string? successMessage, string? errorMessage)> HandleRegistration(RegisterModel model)
        {
            // Send registration request to the Identity API
            var response = await _apiService.PostAsync("Identity", "api/Account/register", model);

            if (response.IsSuccessStatusCode)
            {
                // Registration succeeded
                return ("Registration successful!", null);
            }
            else
            {
                // Read error response from API
                var error = await response.Content.ReadFromJsonAsync<RegisterResponse>();

                // Extract error message (fallback to generic message if null)
                var errorMessage = error?.Message ?? "An unknown error occurred.";

                // Append validation errors if available
                if (error?.Errors != null && error.Errors.Any())
                {
                    errorMessage += " " + string.Join(" ", error.Errors);
                }

                // Return error result
                return (null, errorMessage);
            }
        }
    }
}
