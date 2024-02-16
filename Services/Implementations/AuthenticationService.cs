using AuthService.AsyncDataServices;
using AuthService.Common.Constants;
using AuthService.Common.Dtos;
using AuthService.Common.Dtos.MessageBusDtos;
using AuthService.Common.Exceptions;
using AuthService.Common.Helpers;
using AuthService.Data.Entity;
using AuthService.Data.Repositories.Interfaces;
using AuthService.Security;
using AuthService.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthService.Services.Implementations;
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IMessageBusClient _messageBusClient;
    private readonly SecurityOptions _securityOptions;

    public AuthenticationService(IUserRepository userRepository, IRoleRepository roleRepository,
        IMessageBusClient messageBusClient, SecurityOptions securityOptions)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _messageBusClient = messageBusClient;
        _securityOptions = securityOptions;
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

        PublishCreatedUser(user.Id);

        return await GenerateJwtToken(user);
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

    private async Task<TokenDto> GenerateJwtToken(User user)
    {
        var roles = await _userRepository.GetUserRoles(user.Id);
        var claims = new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) }
                .Concat(roles.OrderBy(e => e!.Id).Select(e => new Claim(ClaimTypes.Role, e?.Name!)));

        var rsa = RSA.Create();
        var key = await File.ReadAllTextAsync(_securityOptions.PrivateKeyFilePath);
        rsa.FromXmlString(key);

        var credentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

        var jwt = new JwtSecurityToken(new JwtHeader(credentials), new JwtPayload(
            "webapi",
            "webapi",
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1)
        ));

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new TokenDto(token);
    }

    private void PublishCreatedUser(int id)
    {
        try
        {
            _messageBusClient.PublishUser(new PublishUserDto() { Id = id });
        }
        catch (Exception)
        {
            Console.WriteLine($"--> Could not send RabbitMQ message");
        }
    }
}