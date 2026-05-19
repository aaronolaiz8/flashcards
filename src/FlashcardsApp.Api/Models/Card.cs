namespace FlashcardsApp.Api.Models;

public class Card
{
    public int Id { get; set; }
    public int DeckId { get; set; }
    public string Front { get; set; } = null!;
    public string Back { get; set; } = null!;
    public string[] Tags { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Deck Deck { get; set; } = null!;
    public ICollection<CardMemoryState> MemoryStates { get; set; } = [];
    public ICollection<ReviewLog> ReviewLogs { get; set; } = [];
}
