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

            builder.HasIndex(e => new { e.DoctorId, e.DayOfWeek }, "IX_utblCMSDoctorSchedules_DoctorId_DayOfWeek");

            builder.Property(e => e.Id).HasMaxLength(36);
            builder.Property(e => e.DoctorId).HasMaxLength(36);
            builder.Property(e => e.IsActive).HasDefaultValue(true);

            builder.HasOne(d => d.Doctor).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}