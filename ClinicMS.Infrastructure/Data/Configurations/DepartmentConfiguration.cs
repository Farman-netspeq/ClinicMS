using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    // IEntityTypeConfiguration<Department> = EF contract
    public class DepartmentConfiguration
        : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            // ── Table name ─────────────────────────────────
            builder.ToTable("utblCMSDepartments");

            // ── Primary Key ────────────────────────────────
            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id)
                .HasMaxLength(36);

            // ── Columns ────────────────────────────────────
            builder.Property(d => d.Name)
                .IsRequired()           
                .HasMaxLength(100);   

            builder.Property(d => d.Description)
                .HasMaxLength(500);     

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true); 

            // ── Unique constraint ──────────────────────────
            // Two departments cannot have same name
            builder.HasIndex(d => d.Name)
                .IsUnique();

            // ── Relationships ──────────────────────────────
            // Department has many Doctors
            // Doctor has one Department (DepartmentId FK)
            // DeleteBehavior.Restrict = cannot delete Department
            // if it has Doctors linked → protects data integrity
            builder.HasMany(d => d.Doctors)
                .WithOne(doc => doc.Department)
                .HasForeignKey(doc => doc.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}