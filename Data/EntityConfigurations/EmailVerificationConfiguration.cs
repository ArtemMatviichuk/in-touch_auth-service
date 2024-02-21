using AuthService.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Data.EntityConfigurations
{
    public class EmailVerificationConfiguration : IEntityTypeConfiguration<EmailVerification>
    {
        public void Configure(EntityTypeBuilder<EmailVerification> builder)
        {
            builder.ToTable("EmailVerifications");

            builder.Property(e => e.Code).IsRequired().HasMaxLength(16);
            builder.Property(e => e.ValidTo)
                    .HasConversion(d => d, d => DateTime.SpecifyKind(d, DateTimeKind.Utc));

            builder.HasOne(e => e.User).WithOne(e => e.Verification).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
