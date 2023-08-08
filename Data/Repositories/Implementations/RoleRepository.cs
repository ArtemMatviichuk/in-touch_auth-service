using AuthService.Data.Entity;
using AuthService.Data.Repositories.Interfaces;

namespace AuthService.Data.Repositories.Implementations;
public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(AuthContext context)
        : base(context)
    {
    }
}