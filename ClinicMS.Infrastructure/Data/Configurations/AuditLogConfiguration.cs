using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<utblCMSAuditLogs>
    {
        public void Configure(EntityTypeBuilder<utblCMSAuditLogs> builder)
        {
            builder.ToTable("utblCMSAuditLogs");

            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).HasMaxLength(36);
            builder.Property(a => a.UserId).IsRequired().HasMaxLength(450);
            builder.Property(a => a.UserName).IsRequired().HasMaxLength(256);
            builder.Property(a => a.Action).IsRequired().HasConversion<int>();
            builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
            builder.Property(a => a.EntityId).IsRequired().HasMaxLength(36);
            builder.Property(a => a.Details).HasMaxLength(1000);
            builder.Property(a => a.TransDate).IsRequired();

            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}