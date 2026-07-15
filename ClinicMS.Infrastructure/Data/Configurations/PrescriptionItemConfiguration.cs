using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
    {
        public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
        {
            builder.ToTable("utblCMSPrescriptionItems");

            builder.Property(e => e.Id).HasMaxLength(36);
            builder.Property(e => e.PrescriptionId).IsRequired().HasMaxLength(36);
            builder.Property(e => e.MedicationName).IsRequired().HasMaxLength(200);
            builder.Property(e => e.Dosage).HasMaxLength(50);
            builder.Property(e => e.Frequency).HasMaxLength(100);
            builder.Property(e => e.Instructions).HasMaxLength(500);
        }
    }
}