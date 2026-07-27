using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            builder.ToTable("utblCMSMedicalRecords");

            builder.Property(e => e.Id).HasMaxLength(36);
            builder.Property(e => e.AppointmentId).IsRequired().HasMaxLength(36);
            builder.Property(e => e.BloodPressure).HasMaxLength(20);
            builder.Property(e => e.Temperature).HasColumnType("decimal(5,2)");
            builder.Property(e => e.Weight).HasColumnType("decimal(5,2)");
            builder.Property(e => e.Height).HasColumnType("decimal(5,2)");
            builder.Property(e => e.Diagnosis).IsRequired().HasMaxLength(1000);
            builder.Property(e => e.Notes).HasMaxLength(2000);
            builder.Property(e => e.LastUpdatedBy).HasMaxLength(450);

            builder.HasIndex(e => e.AppointmentId).IsUnique();   // 1:1 enforcement

            builder.HasOne(e => e.Appointment)
                .WithOne()
                .HasForeignKey<MedicalRecord>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}