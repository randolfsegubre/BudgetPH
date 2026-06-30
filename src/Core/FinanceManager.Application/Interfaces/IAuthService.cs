using FinanceManager.Application.Features.Auth.DTOs;
using FinanceManager.Application.Common.Models;

namespace FinanceManager.Application.Interfaces;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(string userId, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
    Task<Result<UserProfileDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto, CancellationToken cancellationToken = default);
    Task<Result<UserProfileDto>> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
}
