namespace AuthDemo.Sports.Services
{
    /// <summary>
    /// Holds the authentication token and associated user information
    /// for the current session in the Blazor application.
    /// </summary>
    public class TokenHolder
    {
        public string? Token { get; set; }

        public string? UserEmail { get; set; }
    }
}
