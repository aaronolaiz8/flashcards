using FlashcardsApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FlashcardsApp.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<UserAiSettings> UserAiSettings => Set<UserAiSettings>();
    public DbSet<Deck> Decks => Set<Deck>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<CardMemoryState> CardMemoryStates => Set<CardMemoryState>();
    public DbSet<ReviewLog> ReviewLogs => Set<ReviewLog>();
    public DbSet<StudySession> StudySessions => Set<StudySession>();
    public DbSet<Goal> Goals => Set<Goal>();
    public DbSet<GoalDeck> GoalDecks => Set<GoalDeck>();
    public DbSet<GoalProgressLog> GoalProgressLogs => Set<GoalProgressLog>();
    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();
    public DbSet<ScheduledReminder> ScheduledReminders => Set<ScheduledReminder>();
    public DbSet<AiGenerationRequest> AiGenerationRequests => Set<AiGenerationRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasOne(t => t.User).WithMany(u => u.RefreshTokens)
                .HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetToken>(e =>
        {
            e.HasOne(t => t.User).WithMany(u => u.PasswordResetTokens)
                .HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserAiSettings>(e =>
        {
            e.HasOne(s => s.User).WithOne(u => u.AiSettings)
                .HasForeignKey<UserAiSettings>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            e.Property(s => s.Provider).HasConversion<string>();
        });

        modelBuilder.Entity<Deck>(e =>
        {
            e.HasOne(d => d.User).WithMany(u => u.Decks)
                .HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade);
            e.Property(d => d.Visibility).HasConversion<string>();
            e.Property(d => d.Tags).HasColumnType("text[]");
        });

        modelBuilder.Entity<Card>(e =>
        {
            e.HasOne(c => c.Deck).WithMany(d => d.Cards)
                .HasForeignKey(c => c.DeckId).OnDelete(DeleteBehavior.Cascade);
            e.Property(c => c.Tags).HasColumnType("text[]");
        });

        modelBuilder.Entity<CardMemoryState>(e =>
        {
            e.HasOne(s => s.User).WithMany(u => u.CardMemoryStates)
                .HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Card).WithMany(c => c.MemoryStates)
                .HasForeignKey(s => s.CardId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(s => new { s.UserId, s.CardId }).IsUnique();
            e.Property(s => s.State).HasConversion<string>();
        });

        modelBuilder.Entity<ReviewLog>(e =>
        {
            e.HasOne(r => r.Card).WithMany(c => c.ReviewLogs)
                .HasForeignKey(r => r.CardId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Session).WithMany(s => s.ReviewLogs)
                .HasForeignKey(r => r.SessionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudySession>(e =>
        {
            e.HasOne(s => s.User).WithMany(u => u.StudySessions)
                .HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Goal).WithMany(g => g.StudySessions)
                .HasForeignKey(s => s.GoalId).OnDelete(DeleteBehavior.SetNull);
            e.Property(s => s.Mode).HasConversion<string>();
        });

        modelBuilder.Entity<Goal>(e =>
        {
            e.HasOne(g => g.User).WithMany(u => u.Goals)
                .HasForeignKey(g => g.UserId).OnDelete(DeleteBehavior.Cascade);
            e.Property(g => g.Status).HasConversion<string>();
        });

        modelBuilder.Entity<GoalDeck>(e =>
        {
            e.HasOne(gd => gd.Goal).WithMany(g => g.GoalDecks)
                .HasForeignKey(gd => gd.GoalId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(gd => gd.Deck).WithMany(d => d.GoalDecks)
                .HasForeignKey(gd => gd.DeckId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(gd => new { gd.GoalId, gd.DeckId }).IsUnique();
            e.Property(gd => gd.CardFilterTags).HasColumnType("text[]");
        });

        modelBuilder.Entity<GoalProgressLog>(e =>
        {
            e.HasOne(p => p.Goal).WithMany(g => g.ProgressLogs)
                .HasForeignKey(p => p.GoalId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationPreferences>(e =>
        {
            e.HasOne(n => n.User).WithOne(u => u.NotificationPreferences)
                .HasForeignKey<NotificationPreferences>(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
            e.Property(n => n.FrequencyMode).HasConversion<string>();
            e.Property(n => n.Channel).HasConversion<string>();
        });

        modelBuilder.Entity<ScheduledReminder>(e =>
        {
            e.Property(r => r.Type).HasConversion<string>();
            e.Property(r => r.Status).HasConversion<string>();
        });

        modelBuilder.Entity<AiGenerationRequest>(e =>
        {
            e.Property(r => r.Status).HasConversion<string>();
        });
    }
}
