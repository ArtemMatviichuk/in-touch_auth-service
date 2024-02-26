
using AuthService.Common.Constants;
using AuthService.Common.Helpers;
using AuthService.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;
public static class DbPreparator
{
    public static async Task PrepareDb(IApplicationBuilder app, IConfiguration configuration)
    {
        using var serviceScope = app.ApplicationServices.CreateScope();
        await SeedData(serviceScope.ServiceProvider.GetService<AuthContext>()!, configuration);
    }

    private static async Task SeedData(AuthContext authContext, IConfiguration configuration)
    {
        authContext.Database.Migrate();

        if (!authContext.Set<Role>().Any())
        {
            var admin = new User()
            {
                PublicId = HashHelper.GenerateGUID(),
                Email = configuration[AppConstants.AdminEmail],
                PasswordHash = HashHelper.GetHashFromString(configuration[AppConstants.AdminPassword]!),
                RegisteredDate = DateTime.UtcNow,
                Roles = new List<UserRole>()
                {
                    new UserRole() { Role = new Role() { Name = UserRelatedConstants.AdminName, Value = 1 } },
                    new UserRole() { Role = new Role() { Name = UserRelatedConstants.UserName, Value = 2 } },
                }
            };

            await authContext.Set<User>().AddAsync(admin);
            await authContext.SaveChangesAsync();
        }
    }
}