
using AuthService.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Data.EntityConfigurations;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.Property(e => e.PublicId).IsRequired().HasMaxLength(128);
        builder.HasIndex(e => e.PublicId).IsUnique();

        builder.Property(e => e.Email).IsRequired().HasMaxLength(250);
        builder.Property(e => e.PasswordHash).IsRequired().HasMaxLength(250);

        builder.Property(e => e.RegisteredDate)
                .HasConversion(d => d, d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
    }
}