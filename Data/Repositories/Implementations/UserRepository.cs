using AuthService.Data.Entity;
using AuthService.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data.Repositories.Implementations;
public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AuthContext context)
        : base(context)
    {
    }

    public async Task<IEnumerable<Role?>> GetUserRoles(int userId)
    {
        return await _context.Set<UserRole>()
            .AsNoTracking()
            .Include(e => e.Role)
            .Where(e => e.UserId == userId)
            .Select(e => e.Role)
            .OrderBy(e => e!.Value)
            .ToListAsync();
    }
}