using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Retainica.Api.Data;
using Retainica.Api.DTOs.Auth;
using Retainica.Api.Models;
using Retainica.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Retainica.Api.Services;

public class AuthService(AppDbContext db, IConfiguration config, IEmailService emailService) : IAuthService
{
    private readonly string _jwtKey = config["Jwt:Key"]!;
    private readonly string _jwtIssuer = config["Jwt:Issuer"]!;
    private readonly string _jwtAudience = config["Jwt:Audience"]!;
    private readonly string _appBaseUrl = config["App:BaseUrl"] ?? "http://localhost:5173";

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await db.Users.AnyAsync(u => u.Email == request.Email.ToLower()))
            throw new ArgumentException("Email already in use");

        var user = new User
        {
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var tokens = await IssueTokensAsync(user);

        // All DB work is sequential on this scoped DbContext; only the email
        // network send is fire-and-forget (DbContext is not thread-safe).
        await QueueVerificationEmailAsync(user);

        return tokens;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower())
            ?? throw new UnauthorizedAccessException("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash)
            ?? throw new UnauthorizedAccessException("Invalid refresh token");

        if (!stored.IsActive)
            throw new UnauthorizedAccessException("Refresh token expired or revoked");

        stored.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return await IssueTokensAsync(stored.User);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        if (stored != null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
        if (user == null) return; // Silent — don't reveal whether email exists

        var token = GenerateSecureToken();
        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(token),
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
        await db.SaveChangesAsync();

        var resetLink = $"{_appBaseUrl}/reset-password?token={token}";
        await emailService.SendPasswordResetAsync(user.Email, user.DisplayName, resetLink);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var tokenHash = HashToken(request.Token);
        var stored = await db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash)
            ?? throw new ArgumentException("Invalid or expired reset token");

        if (!stored.IsValid)
            throw new ArgumentException("Invalid or expired reset token");

        stored.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        stored.UsedAt = DateTime.UtcNow;

        // Revoke all refresh tokens on password reset
        var tokens = await db.RefreshTokens.Where(t => t.UserId == stored.UserId && t.RevokedAt == null).ToListAsync();
        foreach (var t in tokens) t.RevokedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    public async Task VerifyEmailAsync(string token)
    {
        var tokenHash = HashToken(token);
        var stored = await db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UsedAt == null)
            ?? throw new ArgumentException("Invalid verification token");

        stored.User.EmailVerifiedAt = DateTime.UtcNow;
        stored.UsedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<UserDto> GetProfileAsync(int userId)
    {
        var user = await db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("User not found");
        return MapToDto(user);
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("User not found");
        user.DisplayName = request.DisplayName;
        await db.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("User not found");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new ArgumentException("Current password is incorrect");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(int userId, string password)
    {
        var user = await db.Users.FindAsync(userId) ?? throw new KeyNotFoundException("User not found");

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            throw new ArgumentException("Password is incorrect");

        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }

    private async Task<AuthResponse> IssueTokensAsync(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateSecureToken();

        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });
        await db.SaveChangesAsync();

        return new AuthResponse(accessToken, refreshToken, MapToDto(user));
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwtIssuer,
            audience: _jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSecureToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');

    private static string HashToken(string token) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static UserDto MapToDto(User user) =>
        new(user.Id, user.Email, user.DisplayName, user.Role.ToString(), user.EmailVerifiedAt.HasValue);

    private async Task QueueVerificationEmailAsync(User user)
    {
        // DB write happens on the request's DbContext (sequential, not concurrent).
        var token = GenerateSecureToken();
        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = HashToken(token),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
        await db.SaveChangesAsync();

        var verifyLink = $"{_appBaseUrl}/verify-email?token={token}";
        var email = user.Email;
        var name = user.DisplayName;

        // The email network send is non-blocking and does NOT touch the DbContext.
        _ = Task.Run(async () =>
        {
            try { await emailService.SendEmailVerificationAsync(email, name, verifyLink); }
            catch { /* verification email failure is non-blocking */ }
        });
    }
}
