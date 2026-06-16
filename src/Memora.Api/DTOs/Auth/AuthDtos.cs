namespace Memora.Api.DTOs.Auth;

public record RegisterRequest(string Email, string Password, string DisplayName);
public record LoginRequest(string Email, string Password);
public record ResetPasswordRequest(string Token, string NewPassword);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record AuthResponse(string AccessToken, string RefreshToken, UserDto User);
public record UserDto(int Id, string Email, string DisplayName, string Role, bool EmailVerified);
public record ForgotPasswordRequest(string Email);
public record DeleteAccountRequest(string Password);
public record UpdateProfileRequest(string DisplayName);
