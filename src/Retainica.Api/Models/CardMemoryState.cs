namespace Retainica.Api.Models;

public enum CardState { New, Learning, Review, Relearning }

public class CardMemoryState
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CardId { get; set; }
    public float? Stability { get; set; }
    public float? Difficulty { get; set; }
    public CardState State { get; set; } = CardState.New;
    public DateTime? NextReviewDate { get; set; }
    public DateTime? LastReviewedAt { get; set; }
    public int? LastRating { get; set; }
    public int ReviewCount { get; set; }
    public int LearningStep { get; set; }

    public User User { get; set; } = null!;
    public Card Card { get; set; } = null!;
}
