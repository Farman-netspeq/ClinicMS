using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class DoctorConfiguration : IEntityTypeConfiguration<utblCMSDoctors>
    {
        public void Configure(EntityTypeBuilder<utblCMSDoctors> builder)
        {
            builder.ToTable("utblCMSDoctors");

            builder.HasIndex(e => e.ApplicationUserId, "IX_utblCMSDoctors_ApplicationUserId").IsUnique();
            builder.HasIndex(e => e.DepartmentId, "IX_utblCMSDoctors_DepartmentId");
            builder.HasIndex(e => e.LicenseNumber, "IX_utblCMSDoctors_LicenseNumber").IsUnique();

            builder.Property(e => e.Id).HasMaxLength(36);
            builder.Property(e => e.ConsultationFee).HasColumnType("decimal(18, 2)");
            builder.Property(e => e.DepartmentId).HasMaxLength(36);
            builder.Property(e => e.FullName).HasMaxLength(150);
            builder.Property(e => e.IsActive).HasDefaultValue(true);
            builder.Property(e => e.LicenseNumber).HasMaxLength(50);
            builder.Property(e => e.Specialization).HasMaxLength(100);

            builder.HasOne(d => d.ApplicationUser)
                .WithMany()
                .HasForeignKey(d => d.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Department).WithMany(p => p.Doctors)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}