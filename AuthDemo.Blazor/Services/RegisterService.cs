using AuthDemo.Blazor.Models;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using AuthDemo.Shared.Models;

namespace AuthDemo.Blazor.Services
{
    public class RegisterService
    {
        private readonly ApiService _apiService;

        public RegisterService(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<(string? successMessage, string? errorMessage)> HandleRegistration(RegisterModel model)
        {
            var response = await _apiService.PostAsync("Identity", "api/Account/register", model);

            if (response.IsSuccessStatusCode)
            {
                return ("Registration successful!", null);
            }
            else
            {
                var error = await response.Content.ReadFromJsonAsync<RegisterResponse>();
                var errorMessage = error?.Message ?? "An unknown error occurred.";
                if (error?.Errors != null && error.Errors.Any())
                {
                    errorMessage += " " + string.Join(" ", error.Errors);
                }
                return (null, errorMessage);
            }
        }
    }
}