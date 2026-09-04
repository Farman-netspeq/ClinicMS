using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<utblCMSRefreshToken>
    {
        public void Configure(EntityTypeBuilder<utblCMSRefreshToken> builder)
        {
            builder.ToTable("utblCMSRefreshToken");
            builder.Property(e => e.Id).HasMaxLength(450);
            builder.Property(e => e.Token).IsRequired();
            builder.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            builder.HasIndex(e => e.Token);
        }
    }
}