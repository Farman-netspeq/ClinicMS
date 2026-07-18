using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
    {
        public void Configure(EntityTypeBuilder<Prescription> builder)
        {
            builder.ToTable("utblCMSPrescriptions");

            builder.Property(e => e.Id).HasMaxLength(36);
            builder.Property(e => e.AppointmentId).IsRequired().HasMaxLength(36);
            builder.Property(e => e.PatientId).IsRequired().HasMaxLength(36);
            builder.Property(e => e.DoctorId).IsRequired().HasMaxLength(36);
            builder.Property(e => e.Notes).HasMaxLength(1000);

            builder.HasIndex(e => e.AppointmentId).IsUnique();   // 1:1 enforcement

            builder.HasOne(e => e.Appointment)
                .WithOne()
                .HasForeignKey<Prescription>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Doctor)
                .WithMany()
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Items)
                .WithOne(i => i.Prescription)
                .HasForeignKey(i => i.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}