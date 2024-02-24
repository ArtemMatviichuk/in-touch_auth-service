using AuthService.Common.Dtos;
using AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    public AuthController(IAuthenticationService authenticationService)
    {
        _authService = authenticationService;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] UserAuthRequestDto dto)
    {
        var tokenDto = await _authService.Register(dto);
        return Ok(tokenDto);
    }

    [HttpPost("Authenticate")]
    public async Task<IActionResult> Authenticate([FromBody] UserAuthRequestDto dto)
    {
        var tokenDto = await _authService.Authenticate(dto);
        return Ok(tokenDto);
    }

    [Authorize]
    [HttpPost("AuthData")]
    public async Task<IActionResult> UpdateAuthenticationData([FromBody] UserAuthRequestDto dto)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authService.UpdateAuthenticationData(userId, dto);

        return NoContent();
    }

    [Authorize]
    [HttpPost("SendEmailVerification")]
    public async Task<IActionResult> SendVerificationEmail()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _authService.SendVerificationEmail(userId);

        return Ok(response);
    }

    [Authorize]
    [HttpPost("EmailVerification")]
    public async Task<IActionResult> VerifyEmail([FromBody] ValueDto<string> dto)
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authService.VerifyEmail(userId, dto.Value ?? string.Empty);

        return NoContent();
    }

    [Authorize]
    [HttpGet("EmailVerification")]
    public async Task<IActionResult> CheckIfEmailVerified()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _authService.CheckIfEmailVerified(userId);

        return Ok(response);
    }

    [Authorize]
    [HttpDelete("Account")]
    public async Task<IActionResult> RemoveAccount()
    {
        int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _authService.RemoveAccount(userId);

        return NoContent();
    }
}
