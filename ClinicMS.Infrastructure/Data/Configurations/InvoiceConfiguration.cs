using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("utblCMSInvoices");

            builder.Property(e => e.Id).HasMaxLength(36);
            builder.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(20);
            builder.Property(e => e.PatientId).IsRequired().HasMaxLength(36);
            builder.Property(e => e.AppointmentId).IsRequired().HasMaxLength(36);
            builder.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

            builder.HasIndex(e => e.InvoiceNumber).IsUnique();
            builder.HasIndex(e => e.AppointmentId).IsUnique();
            builder.HasIndex(e => e.PatientId);
            builder.HasIndex(e => e.Status);

            builder.HasOne(e => e.Patient)
                .WithMany()
                .HasForeignKey(e => e.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(e => e.Appointment)
                .WithOne()
                .HasForeignKey<Invoice>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Items)
                .WithOne(i => i.Invoice)
                .HasForeignKey(i => i.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}