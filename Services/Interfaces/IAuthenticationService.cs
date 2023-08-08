using AuthService.Common.Dtos;

namespace AuthService.Services.Interfaces;
public interface IAuthenticationService
{
    Task<TokenDto> Register(UserAuthRequestDto dto);
    Task<TokenDto> Authenticate(UserAuthRequestDto dto);
}