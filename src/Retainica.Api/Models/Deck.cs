namespace Retainica.Api.Models;

public enum DeckVisibility { Private, Unlisted, Public }

public class Deck
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string[] Tags { get; set; } = [];
    public DeckVisibility Visibility { get; set; } = DeckVisibility.Private;
    public int? ForkedFromDeckId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<Card> Cards { get; set; } = [];
    public ICollection<GoalDeck> GoalDecks { get; set; } = [];
}
