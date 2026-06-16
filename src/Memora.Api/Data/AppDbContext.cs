using System.Text.Json;
using Memora.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Memora.Api.Data;

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
        // string[] maps natively to Postgres text[]. SQL Server has no array type,
        // so for non-Postgres providers store the arrays as JSON in nvarchar(max).
        var isNpgsql = Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";

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
            ConfigureStringArray(e.Property(d => d.Tags), isNpgsql);
        });

        modelBuilder.Entity<Card>(e =>
        {
            e.HasOne(c => c.Deck).WithMany(d => d.Cards)
                .HasForeignKey(c => c.DeckId).OnDelete(DeleteBehavior.Cascade);
            ConfigureStringArray(e.Property(c => c.Tags), isNpgsql);
        });

        modelBuilder.Entity<CardMemoryState>(e =>
        {
            // Card is the primary DB cascade path; User is softened to ClientCascade
            // to avoid SQL Server multiple-cascade-paths (User->Decks->Cards->MemoryStates
            // and User->MemoryStates both reach this table). User deletion still removes
            // these rows via the Card path.
            e.HasOne(s => s.User).WithMany(u => u.CardMemoryStates)
                .HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.ClientCascade);
            e.HasOne(s => s.Card).WithMany(c => c.MemoryStates)
                .HasForeignKey(s => s.CardId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(s => new { s.UserId, s.CardId }).IsUnique();
            e.Property(s => s.State).HasConversion<string>();
        });

        modelBuilder.Entity<ReviewLog>(e =>
        {
            // Card is the primary DB cascade path; Session is softened to ClientCascade
            // to avoid SQL Server multiple-cascade-paths into ReviewLogs.
            e.HasOne(r => r.Card).WithMany(c => c.ReviewLogs)
                .HasForeignKey(r => r.CardId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Session).WithMany(s => s.ReviewLogs)
                .HasForeignKey(r => r.SessionId).OnDelete(DeleteBehavior.ClientCascade);
        });

        modelBuilder.Entity<StudySession>(e =>
        {
            // User is the primary DB cascade path; Goal uses ClientSetNull (EF nulls GoalId
            // on tracked sessions; DB FK is NO ACTION) to avoid SQL Server multiple-cascade-paths
            // into StudySessions. Goal deletion still detaches sessions per spec.
            e.HasOne(s => s.User).WithMany(u => u.StudySessions)
                .HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Goal).WithMany(g => g.StudySessions)
                .HasForeignKey(s => s.GoalId).OnDelete(DeleteBehavior.ClientSetNull);
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
            // Goal is the primary DB cascade path; Deck is softened to ClientCascade
            // to avoid SQL Server multiple-cascade-paths into GoalDecks. Per spec, deck
            // removal from a goal is handled in app logic (goal survives, budget recalculated).
            e.HasOne(gd => gd.Goal).WithMany(g => g.GoalDecks)
                .HasForeignKey(gd => gd.GoalId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(gd => gd.Deck).WithMany(d => d.GoalDecks)
                .HasForeignKey(gd => gd.DeckId).OnDelete(DeleteBehavior.ClientCascade);
            e.HasIndex(gd => new { gd.GoalId, gd.DeckId }).IsUnique();
            ConfigureNullableStringArray(e.Property(gd => gd.CardFilterTags), isNpgsql);
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

    private static readonly ValueComparer<string[]> StringArrayComparer = new(
        (a, b) => (a ?? Array.Empty<string>()).SequenceEqual(b ?? Array.Empty<string>()),
        v => v.Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
        v => v.ToArray());

    private static void ConfigureStringArray(
        Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<string[]> property, bool isNpgsql)
    {
        if (isNpgsql)
        {
            property.HasColumnType("text[]");
            return;
        }

        property.HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<string>());
        property.Metadata.SetValueComparer(StringArrayComparer);
    }

    private static void ConfigureNullableStringArray(
        Microsoft.EntityFrameworkCore.Metadata.Builders.PropertyBuilder<string[]?> property, bool isNpgsql)
    {
        if (isNpgsql)
        {
            property.HasColumnType("text[]");
            return;
        }

        property.HasConversion(
            v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => v == null ? null : JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions?)null));
        property.Metadata.SetValueComparer(StringArrayComparer);
    }
}
