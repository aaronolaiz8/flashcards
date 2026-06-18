namespace Retainica.Api.Models;

public class GoalProgressLog
{
    public int Id { get; set; }
    public int GoalId { get; set; }
    public DateOnly Date { get; set; }
    public int CardsOnPace { get; set; }
    public int CardsBehind { get; set; }
    public int CardsAhead { get; set; }
    public float ProjectedMasteryPct { get; set; }
    public int SessionsCompleted { get; set; }

    public Goal Goal { get; set; } = null!;
}
