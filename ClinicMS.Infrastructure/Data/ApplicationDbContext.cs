using ClinicMS.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ClinicMS.Infrastructure.Data
{
    // IdentityDbContext<ApplicationUser> = DbContext that ALSO includes
    // all Identity tables (AspNetUsers, AspNetRoles, AspNetUserRoles etc.)
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor — receives options (connection string etc.) from DI
        // and passes it up to base DbContext. You never call this manually.
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}