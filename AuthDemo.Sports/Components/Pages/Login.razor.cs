using AuthDemo.Sports.Services;
using AuthDemo.Shared.Models;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace AuthDemo.Sports.Components.Pages
{
    public partial class Login : ComponentBase
    {
        [Inject]
        private LoginService LoginService { get; set; }

        [SupplyParameterFromForm]
        protected LoginModel model { get; set; } = new();
        protected string? errorMessage;

        // HandleLogin logic is now in Login.razor for UI enhancements
    }
}