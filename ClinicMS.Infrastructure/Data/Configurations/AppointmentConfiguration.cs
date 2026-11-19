using ClinicMS.Domain.Entities;
using ClinicMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class AppointmentConfiguration
        : IEntityTypeConfiguration<utblCMSAppointments>
    {
        public void Configure(EntityTypeBuilder<utblCMSAppointments> builder)
        {
            builder.ToTable("utblCMSAppointments");

            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id)
                .HasMaxLength(36);

            builder.Property(a => a.AppointmentNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(a => a.AppointmentNumber)
                .IsUnique();

            builder.Property(a => a.PatientId)
                .IsRequired()
                .HasMaxLength(36);

            builder.Property(a => a.DoctorId)
                .IsRequired()
                .HasMaxLength(36);

            builder.Property(a => a.DepartmentId)
                .IsRequired()
                .HasMaxLength(36);

            builder.Property(a => a.AppointmentDate)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(a => a.StartTime)
                .IsRequired()
                .HasColumnType("time");

            builder.Property(a => a.EndTime)
                .IsRequired()
                .HasColumnType("time");

            builder.Property(a => a.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(AppointmentStatus.Scheduled);

            builder.Property(a => a.ChiefComplaint)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(a => a.CancelReason)
                .HasMaxLength(500);

            builder.Property(a => a.TransDate)
      .IsRequired();

            builder.Property(a => a.LastUpdatedBy)
                .IsRequired()
                .HasMaxLength(450);

            // ── Relationships ──────────────────────────────

            // Appointment → Patient
            // Restrict: cannot delete patient with appointments
            builder.HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment → Doctor
            builder.HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment → Department
            builder.HasOne(a => a.Department)
                .WithMany()
                .HasForeignKey(a => a.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment → CreatedBy (ApplicationUser)
            builder.HasOne(a => a.CreatedBy)
    .WithMany()
    .HasForeignKey(a => a.LastUpdatedBy)
    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}