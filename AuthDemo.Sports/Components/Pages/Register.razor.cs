using AuthDemo.Shared.Models;
using AuthDemo.Sports.Services;
using Microsoft.AspNetCore.Components;

namespace AuthDemo.Sports.Components.Pages
{
    public partial class Register : ComponentBase
    {
        [Inject]
        private RegisterService RegisterService { get; set; }

        [SupplyParameterFromForm]
        protected RegisterModel model { get; set; } = new();
        protected string? errorMessage;
        protected string? successMessage;

        protected async Task HandleRegistration()
        {
            var (success, error) = await RegisterService.HandleRegistration(model);
            successMessage = success;
            errorMessage = error;
            if (successMessage != null)
            {
                model = new RegisterModel();
            }
        }
    }
}