using FinanceManager.Application.Interfaces;
using FinanceManager.Application.Features.Auth.DTOs;
using FinanceManager.Application.Common.Models;
using FinanceManager.Domain.Entities;
using FinanceManager.Domain.Enums;
using FinanceManager.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FinanceManager.Infrastructure.Identity;

public class AuthService(
    UserManager<IdentityUser> userManager,
    ApplicationDbContext context,
    IConfiguration configuration) : IAuthService
{
    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default)
    {
        if (await userManager.FindByEmailAsync(dto.Email) != null)
            return Result<AuthResponseDto>.Failure("An account with this email already exists.");

        if (dto.Password != dto.ConfirmPassword)
            return Result<AuthResponseDto>.Failure("Passwords do not match.");

        var identityUser = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        var result = await userManager.CreateAsync(identityUser, dto.Password);
        if (!result.Succeeded)
            return Result<AuthResponseDto>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

        var appUser = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            IdentityUserId = identityUser.Id,
            TimeZone = "Asia/Manila",
            PreferredCurrency = Currency.PHP
        };

        context.AppUsers.Add(appUser);
        await context.SaveChangesAsync(cancellationToken);

        return await GenerateAuthResponseAsync(identityUser, appUser);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByEmailAsync(dto.Email);
        if (identityUser == null || !await userManager.CheckPasswordAsync(identityUser, dto.Password))
            return Result<AuthResponseDto>.Failure("Invalid email or password.");

        var appUser = await context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id, cancellationToken);
        if (appUser == null)
            return Result<AuthResponseDto>.Failure("User profile not found.");

        appUser.LastLoginAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return await GenerateAuthResponseAsync(identityUser, appUser);
    }

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedToken = await context.Set<RefreshToken>().Include(r => r.IdentityUser)
            .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
            return Result<AuthResponseDto>.Failure("Invalid or expired refresh token.");

        storedToken.IsRevoked = true;
        var appUser = await context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == storedToken.UserId, cancellationToken);
        if (appUser == null)
            return Result<AuthResponseDto>.Failure("User not found.");

        await context.SaveChangesAsync(cancellationToken);
        return await GenerateAuthResponseAsync(storedToken.IdentityUser, appUser);
    }

    public async Task<Result> LogoutAsync(string userId, CancellationToken cancellationToken = default)
    {
        var tokens = await context.Set<RefreshToken>().Where(r => r.UserId == userId && !r.IsRevoked).ToListAsync(cancellationToken);
        tokens.ForEach(t => t.IsRevoked = true);
        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            return Result.Failure("New passwords do not match.");

        var identityUser = await userManager.FindByIdAsync(userId);
        if (identityUser == null)
            return Result.Failure("User not found.");

        var result = await userManager.ChangePasswordAsync(identityUser, dto.CurrentPassword, dto.NewPassword);
        return result.Succeeded ? Result.Success() : Result.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    public async Task<Result<UserProfileDto>> UpdateProfileAsync(string userId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
    {
        var identityUser = await userManager.FindByIdAsync(userId);
        if (identityUser == null)
            return Result<UserProfileDto>.Failure("User not found.");

        var appUser = await context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == userId, cancellationToken);
        if (appUser == null)
            return Result<UserProfileDto>.Failure("User profile not found.");

        appUser.FirstName = dto.FirstName;
        appUser.LastName = dto.LastName;
        appUser.PhoneNumber = dto.PhoneNumber;
        appUser.TimeZone = dto.TimeZone;

        if (Enum.TryParse<Currency>(dto.PreferredCurrency, out var currency))
            appUser.PreferredCurrency = currency;

        await context.SaveChangesAsync(cancellationToken);
        return Result<UserProfileDto>.Success(MapToProfileDto(appUser));
    }

    public async Task<Result<UserProfileDto>> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var appUser = await context.AppUsers.FirstOrDefaultAsync(u => u.IdentityUserId == userId, cancellationToken);
        if (appUser == null)
            return Result<UserProfileDto>.Failure("User not found.");
        return Result<UserProfileDto>.Success(MapToProfileDto(appUser));
    }

    private async Task<Result<AuthResponseDto>> GenerateAuthResponseAsync(IdentityUser identityUser, ApplicationUser appUser)
    {
        var (accessToken, expiresAt) = GenerateAccessToken(identityUser, appUser);
        var refreshToken = await GenerateRefreshTokenAsync(identityUser.Id);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            accessToken,
            refreshToken,
            expiresAt,
            MapToProfileDto(appUser)
        ));
    }

    private (string token, DateTime expiresAt) GenerateAccessToken(IdentityUser identityUser, ApplicationUser appUser)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var expiresAt = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiresInMinutes"] ?? "60"));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, identityUser.Id),
            new Claim(JwtRegisteredClaimNames.Email, identityUser.Email!),
            new Claim("appUserId", appUser.Id.ToString()),
            new Claim("fullName", appUser.FullName),
            new Claim("currency", appUser.PreferredCurrency.ToString()),
            new Claim("timezone", appUser.TimeZone ?? "Asia/Manila"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    private async Task<string> GenerateRefreshTokenAsync(string userId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false
        };
        context.Set<RefreshToken>().Add(refreshToken);
        await context.SaveChangesAsync();
        return token;
    }

    private static UserProfileDto MapToProfileDto(ApplicationUser u) =>
        new(u.Id, u.FirstName, u.LastName, u.Email, u.PhoneNumber, u.AvatarUrl,
            u.PreferredCurrency.ToString(), u.TimeZone ?? "Asia/Manila");
}
