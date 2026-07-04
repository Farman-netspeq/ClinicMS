using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class DoctorConfiguration
        : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            // DB naming convention: utblCMS + Doctors
            builder.ToTable("utblCMSDoctors");

            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id)
                .HasMaxLength(36);

            // ── Columns ────────────────────────────────────
            builder.Property(d => d.ApplicationUserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.Property(d => d.FullName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(d => d.DepartmentId)
                .IsRequired()
                .HasMaxLength(36);

            builder.Property(d => d.Specialization)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(d => d.LicenseNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(d => d.ConsultationFee)
                .HasColumnType("decimal(18,2)");

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // ── Unique constraints ─────────────────────────
            builder.HasIndex(d => d.LicenseNumber)
                .IsUnique();
            // No two doctors can have same license number

            builder.HasIndex(d => d.ApplicationUserId)
                .IsUnique();
            // One login account = one doctor profile

            // ── Relationships ──────────────────────────────

            // Doctor → ApplicationUser (login account)
            // Restrict: cannot delete user if doctor profile exists
            builder.HasOne(d => d.ApplicationUser)
                .WithMany()
                .HasForeignKey(d => d.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor → DepartmentId FK

            // Doctor has many Schedules
            builder.HasMany(d => d.Schedules)
                .WithOne(s => s.Doctor)
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}