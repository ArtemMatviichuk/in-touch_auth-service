using AuthService.Common.Constants;
using AuthService.Common.Dtos;
using AuthService.Common.Exceptions;
using AuthService.Common.Helpers;
using AuthService.Data.Entity;
using AuthService.Data.Repositories.Interfaces;
using AuthService.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services.Implementations;
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IConfiguration _configuration;

    public AuthenticationService(IUserRepository userRepository, IRoleRepository roleRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _configuration = configuration;
    }

    public async Task<TokenDto> Authenticate(UserAuthRequestDto dto)
    {
        var user = await _userRepository.Get(e => e.Email!.ToLower() == dto.Email!.ToLower());
        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        if (!HashHelper.IsEqual(user.PasswordHash!, dto.Password!))
        {
            throw new AccessDeniedException("Wrong password");
        }

        return await GenerateJwtToken(user);
    }

    public async Task<TokenDto> Register(UserAuthRequestDto dto)
    {
        var user = await _userRepository.Get(e => e.Email!.ToLower() == dto.Email!.ToLower());
        if (user != null)
        {
            throw new ValidationException("Email is already used");
        }

        var role = await _roleRepository.Get(e => e.Value == UserRelatedConstants.UserRoleValue);
        user = new User()
        {
            Email = dto.Email,
            PasswordHash = HashHelper.GetHashFromString(dto.Password!),
            RegisteredDate = DateTime.UtcNow,
            Roles = new List<UserRole>()
            {
                new UserRole() { RoleId = role!.Id },
            }
        };

        await _userRepository.Add(user);
        await _roleRepository.SaveChanges();

        return await GenerateJwtToken(user);
    }

    private async Task<TokenDto> GenerateJwtToken(User user)
    {
        var roles = await _userRepository.GetUserRoles(user.Id);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration[AppConstants.TokenSecret]!);

        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, string.Join(",", roles.OrderBy(e => e!.Id).Select(e => e!.Name))),
            };

        var tokenDescription = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                 SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescription));
        return new TokenDto(token);
    }
}