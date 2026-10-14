using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<utblCMSPatients>
    {
        public void Configure(EntityTypeBuilder<utblCMSPatients> builder)
        {
            builder.ToTable("utblCMSPatients");

            builder.HasIndex(e => new { e.FirstName, e.LastName }, "IX_utblCMSPatients_FirstName_LastName");
            builder.HasIndex(e => e.PatientNumber, "IX_utblCMSPatients_PatientNumber").IsUnique();
            builder.HasIndex(e => e.Phone, "IX_utblCMSPatients_Phone");

            builder.Property(e => e.Address).HasMaxLength(500);
            builder.Property(e => e.LastUpdatedBy).HasMaxLength(450);
            builder.Property(e => e.Email).HasMaxLength(150);
            builder.Property(e => e.FirstName).HasMaxLength(100);
            builder.Property(e => e.IsActive).HasDefaultValue(true);
            builder.Property(e => e.LastName).HasMaxLength(100);
            builder.Property(e => e.PatientNumber).HasMaxLength(20);
            builder.Property(e => e.Phone).HasMaxLength(20);
        }
    }
}