using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthDemo.Data
{
    /// <summary>
    /// Represents the application's database context.
    /// Inherits from <see cref="IdentityDbContext{TUser}"/> to provide Identity and authentication-related tables.
    /// Also configures OpenIddict entities for token management and authorization.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ApplicationDbContext"/>.
        /// </summary>
        /// <param name="options">The database context configuration options.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Configures the entity model for the database.
        /// This method is called when the database model is being created.
        /// </summary>
        /// <param name="builder">The <see cref="ModelBuilder"/> used to configure entity mappings.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Call the base Identity model creation logic (creates Identity tables)
            base.OnModelCreating(builder);

            // Register OpenIddict entity sets (applications, authorizations, scopes, tokens)
            builder.UseOpenIddict();
        }
    }
}
