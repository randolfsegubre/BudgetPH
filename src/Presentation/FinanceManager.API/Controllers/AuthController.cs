using FinanceManager.Application.Features.Auth.DTOs;
using FinanceManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[Route("api/auth")]
public class AuthController(IAuthService authService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
        => HandleResult(await authService.RegisterAsync(dto, ct));

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
        => HandleResult(await authService.LoginAsync(dto, ct));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto, CancellationToken ct)
        => HandleResult(await authService.RefreshTokenAsync(dto.RefreshToken, ct));

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
        => HandleResult(await authService.LogoutAsync(CurrentUserId, ct));

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
        => HandleResult(await authService.GetProfileAsync(CurrentUserId, ct));

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken ct)
        => HandleResult(await authService.UpdateProfileAsync(CurrentUserId, dto, ct));

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken ct)
        => HandleResult(await authService.ChangePasswordAsync(CurrentUserId, dto, ct));
}
