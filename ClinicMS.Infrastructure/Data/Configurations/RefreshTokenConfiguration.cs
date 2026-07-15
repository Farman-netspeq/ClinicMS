using ClinicMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicMS.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");
            builder.Property(e => e.Id).HasMaxLength(450);
            builder.Property(e => e.Token).IsRequired();
            builder.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            builder.HasIndex(e => e.Token);
        }
    }
}