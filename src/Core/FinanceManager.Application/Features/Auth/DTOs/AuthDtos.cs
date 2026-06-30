namespace FinanceManager.Application.Features.Auth.DTOs;

public record RegisterDto(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string ConfirmPassword,
    string? PhoneNumber
);

public record LoginDto(
    string Email,
    string Password,
    bool RememberMe = false
);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserProfileDto User
);

public record UserProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    string? AvatarUrl,
    string PreferredCurrency,
    string TimeZone
);

public record RefreshTokenDto(string RefreshToken);

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);

public record UpdateProfileDto(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string PreferredCurrency,
    string TimeZone
);
