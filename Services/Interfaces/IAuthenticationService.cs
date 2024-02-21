using AuthService.Common.Dtos;

namespace AuthService.Services.Interfaces;
public interface IAuthenticationService
{
    Task<TokenDto> Register(UserAuthRequestDto dto);
    Task<TokenDto> Authenticate(UserAuthRequestDto dto);
    Task UpdateAuthenticationData(int userId, UserAuthRequestDto dto);
    Task SendVerificationEmail(int userId);
    Task VerifyEmail(int userId, string code);
    Task<ValueDto<bool>> CheckIfEmailVerified(int userId);
}