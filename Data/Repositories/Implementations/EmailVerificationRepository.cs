using AuthService.Data.Entity;
using AuthService.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data.Repositories.Implementations
{
    public class EmailVerificationRepository : Repository<EmailVerification>, IEmailVerificationRepository
    {
        public EmailVerificationRepository(AuthContext context)
            : base(context)
        {
        }

        public async Task<EmailVerification?> GetByUserId(int userId)
        {
            return await _context.Set<EmailVerification>()
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }
    }
}
