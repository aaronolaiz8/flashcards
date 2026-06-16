using Memora.Api.DTOs.Goals;

namespace Memora.Api.Services.Interfaces;

public interface IGoalService
{
    Task<List<GoalDto>> GetGoalsAsync(int userId);
    Task<GoalDto> GetGoalAsync(int goalId, int userId);
    Task<GoalDto> CreateGoalAsync(int userId, CreateGoalRequest request);
    Task<GoalDto> UpdateGoalAsync(int goalId, int userId, UpdateGoalRequest request);
    Task DeleteGoalAsync(int goalId, int userId);
    Task<GoalScheduleDto> GetScheduleAsync(int goalId, int userId);
    Task<List<AtRiskCardDto>> GetAtRiskCardsAsync(int goalId, int userId);
    Task<GoalDto> ExtendGoalAsync(int goalId, int userId, ExtendGoalRequest request);
}
