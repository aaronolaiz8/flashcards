namespace FlashcardsApp.Api.Models;

public class GoalDeck
{
    public int Id { get; set; }
    public int GoalId { get; set; }
    public int DeckId { get; set; }
    public string[]? CardFilterTags { get; set; }

    public Goal Goal { get; set; } = null!;
    public Deck Deck { get; set; } = null!;
}
