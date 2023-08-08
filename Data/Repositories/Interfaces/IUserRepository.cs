using AuthService.Data.Entity;

namespace AuthService.Data.Repositories.Interfaces;
public interface IUserRepository : IRepository<User>
{
    Task<IEnumerable<Role?>> GetUserRoles(int userId);
}