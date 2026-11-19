using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class InvoiceItemConfiguration : IEntityTypeConfiguration<utblCMSInvoiceItems>
    {
        public void Configure(EntityTypeBuilder<utblCMSInvoiceItems> builder)
        {
            builder.ToTable("utblCMSInvoiceItems");

            builder.Property(e => e.Id).HasMaxLength(36);
            builder.Property(e => e.InvoiceId).IsRequired().HasMaxLength(36);
            builder.Property(e => e.Description).IsRequired().HasMaxLength(300);
            builder.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Property(e => e.LineTotal).HasColumnType("decimal(18,2)");
        }
    }
}