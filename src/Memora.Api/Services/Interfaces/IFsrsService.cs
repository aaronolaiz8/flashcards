using Memora.Api.Models;

namespace Memora.Api.Services.Interfaces;

public interface IFsrsService
{
    CardMemoryState ProcessReview(CardMemoryState state, int rating, DateTime reviewedAt);
    float ComputeRetrievability(CardMemoryState state, DateTime asOf);
    List<CardMemoryState> GetDueCards(IEnumerable<CardMemoryState> states, int? goalId, DateTime asOf);
}
