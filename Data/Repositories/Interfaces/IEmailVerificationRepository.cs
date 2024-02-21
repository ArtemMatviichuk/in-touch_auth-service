using AuthService.Data.Entity;

namespace AuthService.Data.Repositories.Interfaces
{
    public interface IEmailVerificationRepository : IRepository<EmailVerification>
    {
        Task<EmailVerification?> GetByUserId(int userId);
    }
}
