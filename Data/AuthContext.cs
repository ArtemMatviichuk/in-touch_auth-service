using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;
public class AuthContext : DbContext
{
    public AuthContext(DbContextOptions<AuthContext> opt)
        : base(opt)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}