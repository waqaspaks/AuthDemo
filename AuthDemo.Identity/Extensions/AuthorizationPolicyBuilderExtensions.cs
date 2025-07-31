using Microsoft.AspNetCore.Authorization;

public static class AuthorizationPolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder builder, string requiredScope)
    {
        return builder.RequireAssertion(context =>
            context.User.HasClaim(c =>
                c.Type == "scope" &&
                (c.Value == requiredScope || c.Value.Split(' ').Contains(requiredScope))));
    }
}
