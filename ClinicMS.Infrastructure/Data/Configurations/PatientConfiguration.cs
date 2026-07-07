using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("utblCMSPatients");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).HasMaxLength(450);

            builder.Property(p => p.PatientNumber)
                .IsRequired()
                .HasMaxLength(20);
            builder.HasIndex(p => p.PatientNumber).IsUnique();
            // unique index — DB itself rejects duplicate patient numbers,

            builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            builder.Property(p => p.DateOfBirth).IsRequired();
            builder.Property(p => p.Gender).IsRequired();
            builder.Property(p => p.BloodGroup).IsRequired();

            builder.Property(p => p.Phone).IsRequired().HasMaxLength(20);
            builder.Property(p => p.Email).HasMaxLength(150);
            builder.Property(p => p.Address).HasMaxLength(500);

            builder.Property(p => p.IsActive).HasDefaultValue(true);
            builder.Property(p => p.CreatedOn).IsRequired();
            builder.Property(p => p.CreatedById).HasMaxLength(450);

            // index to speed up search-by-name/phone 
            builder.HasIndex(p => p.Phone);
            builder.HasIndex(p => new { p.FirstName, p.LastName });
        }
    }
}