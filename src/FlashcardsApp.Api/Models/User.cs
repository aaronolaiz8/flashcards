namespace FlashcardsApp.Api.Models;

public enum UserRole { User, Admin }

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EmailVerifiedAt { get; set; }
    public TimeOnly? PreferredStudyTime { get; set; }
    public TimeOnly? InferredStudyTime { get; set; }
    public TimeOnly? QuietHoursStart { get; set; }
    public TimeOnly? QuietHoursEnd { get; set; }
    public string? Timezone { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = [];
    public ICollection<Deck> Decks { get; set; } = [];
    public ICollection<CardMemoryState> CardMemoryStates { get; set; } = [];
    public ICollection<StudySession> StudySessions { get; set; } = [];
    public ICollection<Goal> Goals { get; set; } = [];
    public UserAiSettings? AiSettings { get; set; }
    public NotificationPreferences? NotificationPreferences { get; set; }
}
