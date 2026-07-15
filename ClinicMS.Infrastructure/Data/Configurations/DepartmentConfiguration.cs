using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("utblCMSDepartments");

            builder.HasIndex(e => e.Name, "IX_utblCMSDepartments_Name").IsUnique();

            builder.Property(e => e.Id).HasMaxLength(36);
            builder.Property(e => e.Description).HasMaxLength(500);
            builder.Property(e => e.IsActive).HasDefaultValue(true);
            builder.Property(e => e.Name).HasMaxLength(100);
        }
    }
}