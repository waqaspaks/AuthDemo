using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Authorization Policy Extension
/// </summary>
public static class AuthorizationPolicyBuilderExtensions
{
    /// <summary>
    /// Adds a requirement to the <see cref="AuthorizationPolicyBuilder"/> that ensures the user has the specified scope.
    /// This method checks the "scope" claim in the user's token and validates it against the required scope.
    /// </summary>
    /// <param name="builder">The authorization policy builder instance.</param>
    /// <param name="requiredScope">The scope that must be present in the user's token.</param>
    /// <returns>The updated <see cref="AuthorizationPolicyBuilder"/> with the scope requirement.</returns>
    public static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder builder, string requiredScope)
    {
        return builder.RequireAssertion(context =>
            context.User.HasClaim(c =>
                c.Type == "scope" &&
                (c.Value == requiredScope || c.Value.Split(' ').Contains(requiredScope))));
    }
}
