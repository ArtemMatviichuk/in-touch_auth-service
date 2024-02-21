using AuthService.AppSettingsOptions;
using AuthService.AsyncDataServices.Auth;
using AuthService.Common.Constants;
using AuthService.Common.Dtos;
using AuthService.Common.Dtos.MessageBusDtos;
using AuthService.Common.Exceptions;
using AuthService.Common.Helpers;
using AuthService.Data.Entity;
using AuthService.Data.Repositories.Interfaces;
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
    private readonly IEmailVerificationRepository _emailVerificationRepository;

    private readonly IAuthMessageBusClient _authBusClient;
    private readonly IEmailMessageBusClient _emailBusClient;
    private readonly SecurityOptions _securityOptions;

    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(IUserRepository userRepository, IRoleRepository roleRepository,
        IEmailVerificationRepository emailVerificationRepository, SecurityOptions securityOptions,
        IAuthMessageBusClient authBusClient, IEmailMessageBusClient emailBusClient,
        ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _emailVerificationRepository = emailVerificationRepository;

        _authBusClient = authBusClient;
        _emailBusClient = emailBusClient;
        _securityOptions = securityOptions;

        _logger = logger;
    }

    public async Task<TokenDto> Register(UserAuthRequestDto dto)
    {
        var user = await _userRepository.Get(e => e.Email!.ToLower() == dto.Email!.ToLower());
        if (user != null)
        {
            throw new ValidationException("Email is already used");
        }

        var dateNow = DateTime.UtcNow;
        var role = await _roleRepository.Get(e => e.Value == UserRelatedConstants.UserRoleValue);
        user = new User()
        {
            Email = dto.Email,
            PasswordHash = HashHelper.GetHashFromString(dto.Password!),
            RegisteredDate = dateNow,
            Roles = new List<UserRole>()
            {
                new() { RoleId = role!.Id },
            },
            IsEmailVerified = false,
            Verification = new EmailVerification()
            {
                Code = HashHelper.GenerateVerificationCode(),
                ValidTo = dateNow.AddHours(1),
            },
        };

        await _userRepository.Add(user);
        await _roleRepository.SaveChanges();

        PublishCreatedUser(user.Id);
        await SendVerificationEmail(user.Id);

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

    public async Task UpdateAuthenticationData(int userId, UserAuthRequestDto dto)
    {
        var user = await _userRepository.GetAsTracking(userId);
        if (user == null)
            throw new NotFoundException("User not found!");

        bool emailChnage = user.Email!.ToLower() != dto.Email!.ToLower();

        user.Email = dto.Email;
        user.PasswordHash = HashHelper.GetHashFromString(dto.Password!);

        if (emailChnage)
        {
            user.IsEmailVerified = false;
            var existing = await _emailVerificationRepository.GetByUserId(userId);
            if (existing != null)
            {
                _emailVerificationRepository.Remove(existing);
            }

            user.Verification = new EmailVerification()
            {
                Code = HashHelper.GenerateVerificationCode(),
                ValidTo = DateTime.UtcNow.AddHours(1),
            };
        }

        await _userRepository.SaveChanges();

        try
        {
            await SendVerificationEmail(userId);
        }
        catch (CustomException ex)
        {
            throw new ValidationException(
                $"Data updated successfully, but there is an error sending email verification email: \n{ex.Message}");
        }
    }

    public async Task SendVerificationEmail(int userId)
    {
        var user = await _userRepository.Get(userId);
        if (user == null)
            throw new NotFoundException("User not found");

        if (user.IsEmailVerified)
            throw new ValidationException("Email is already verified");

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ValidationException("Empty email address");

        var verification = await _emailVerificationRepository.GetByUserId(userId);
        if (verification == null)
            throw new NotFoundException("Email verification data not found");

        VerifyEmailDto dto = new()
        {
            Email = user.Email,
            VerificationCode = verification.Code,
        };

        try
        {
            _emailBusClient.SendEmailConfirmationMessage(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Could not send RabbitMQ message: \n{ex.Message}");
        }
    }

    public async Task VerifyEmail(int userId, string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException("Verification code can not be empty");

        var user = await _userRepository.GetAsTracking(userId);
        if (user == null)
            throw new NotFoundException("User not found");

        if (user.IsEmailVerified)
            throw new ValidationException("Email is already verified");

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ValidationException("Empty email address");

        var verification = await _emailVerificationRepository.GetByUserId(userId);
        if (verification == null)
            throw new NotFoundException("Email verification data not found");

        if (verification.Code.ToLower() != code.ToLower())
            throw new ValidationException("Invalid verification code");

        user.IsEmailVerified = true;
        
        _emailVerificationRepository.Remove(verification);
        await _emailVerificationRepository.SaveChanges();
    }

    public async Task<ValueDto<bool>> CheckIfEmailVerified(int userId)
    {
        var user = await _userRepository.Get(userId);
        if (user == null)
            throw new NotFoundException("User not found");

        return new ValueDto<bool>(user.IsEmailVerified);
    }

    private async Task<TokenDto> GenerateJwtToken(User user)
    {
        var roles = await _userRepository.GetUserRoles(user.Id);

        var claims = roles.OrderBy(e => e!.Id).Select(e => new Claim(ClaimTypes.Role, e?.Name!)).ToList();
        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

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
            _authBusClient.PublishUser(new PublishUserDto() { Id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Could not send RabbitMQ message: \n{ex.Message}");
        }
    }
}