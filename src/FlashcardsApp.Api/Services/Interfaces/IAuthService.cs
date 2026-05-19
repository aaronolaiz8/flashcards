using FlashcardsApp.Api.DTOs.Auth;

namespace FlashcardsApp.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshAsync(string refreshToken);
    Task LogoutAsync(string refreshToken);
    Task ForgotPasswordAsync(string email);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task VerifyEmailAsync(string token);
    Task<UserDto> GetProfileAsync(int userId);
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task DeleteAccountAsync(int userId, string password);
}
