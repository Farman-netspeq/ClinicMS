using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
    {
        public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
        {
            builder.ToTable("utblCMSDoctorSchedules");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.DoctorId)
                .IsRequired()
                .HasMaxLength(36);
            builder.Property(s => s.Id).HasMaxLength(36);

            builder.HasOne(s => s.Doctor)
                .WithMany(d => d.Schedules)
                .HasForeignKey(s => s.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(s => s.DayOfWeek).IsRequired();
            builder.Property(s => s.StartTime).IsRequired();
            builder.Property(s => s.EndTime).IsRequired();
            builder.Property(s => s.SlotDurationMinutes).IsRequired();
            builder.Property(s => s.IsActive).HasDefaultValue(true);
        }
    }
}