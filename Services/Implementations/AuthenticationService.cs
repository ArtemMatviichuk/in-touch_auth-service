using AuthService.AppSettingsOptions;
using AuthService.AsyncDataServices.Interfaces;
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
using System.Security.Cryptography;

namespace AuthService.Services.Implementations;
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IEmailVerificationRepository _emailVerificationRepository;

    private readonly IMessagePublisher _publisher;
    private readonly SecurityOptions _securityOptions;

    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(IUserRepository userRepository, IRoleRepository roleRepository,
        IEmailVerificationRepository emailVerificationRepository, SecurityOptions securityOptions,
        IMessagePublisher publisher, ILogger<AuthenticationService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _emailVerificationRepository = emailVerificationRepository;

        _publisher = publisher;
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
            PublicId = HashHelper.GenerateGUID(),
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

        _publisher.PublishCreatedUser(user.Id, user.PublicId);
        _publisher.PublishEmailVerification(user.Email!, user.Verification.Code, user.Verification.ValidTo);

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

    public async Task<ValueDto<DateTime>> SendVerificationEmail(int userId)
    {
        var user = await _userRepository.Get(userId);
        if (user == null)
            throw new NotFoundException("User not found");

        if (user.IsEmailVerified)
            throw new ValidationException("Email is already verified");

        if (string.IsNullOrWhiteSpace(user.Email))
            throw new ValidationException("Empty email address");

        var existing = await _emailVerificationRepository.GetByUserId(userId);
        if (existing != null)
            _emailVerificationRepository.Remove(existing);

        var verification = new EmailVerification()
        {
            Code = HashHelper.GenerateVerificationCode(),
            ValidTo = DateTime.UtcNow.AddHours(1),
            UserId = userId,
        };

        await _emailVerificationRepository.Add(verification);
        await _emailVerificationRepository.SaveChanges();

        _publisher.PublishEmailVerification(user.Email, verification.Code, verification.ValidTo);

        return new ValueDto<DateTime>(verification.ValidTo);
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

        if (verification.ValidTo < DateTime.UtcNow)
            throw new ValidationException("The code has expired");

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

    public async Task RemoveAccount(int userId)
    {
        var user = await _userRepository.Get(userId);
        if (user == null)
            throw new NotFoundException("User not found");

        await _userRepository.Remove(userId);
        await _userRepository.SaveChanges();

        _publisher.PublishRemovedUser(userId);
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
            _securityOptions.Issuer,
            _securityOptions.Audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1)
        ));

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return new TokenDto(token);
    }
}
