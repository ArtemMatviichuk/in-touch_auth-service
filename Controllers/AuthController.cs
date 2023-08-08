using AuthService.Common.Dtos;
using AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    [HttpHead("ValidateToken")]
    public IActionResult ValidateToken()
    {
        return NoContent();
    }

    [Authorize(Policy = "Administrator")]
    [HttpHead("ValidateAdmin")]
    public IActionResult ValidateAdminRole()
    {
        return NoContent();
    }
}
