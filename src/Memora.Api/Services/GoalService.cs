using Memora.Api.Data;
using Memora.Api.DTOs.Goals;
using Memora.Api.Services.Interfaces;

namespace Memora.Api.Services;

public class GoalService(AppDbContext db) : IGoalService
{
    public Task<List<GoalDto>> GetGoalsAsync(int userId) => throw new NotImplementedException();
    public Task<GoalDto> GetGoalAsync(int goalId, int userId) => throw new NotImplementedException();
    public Task<GoalDto> CreateGoalAsync(int userId, CreateGoalRequest request) => throw new NotImplementedException();
    public Task<GoalDto> UpdateGoalAsync(int goalId, int userId, UpdateGoalRequest request) => throw new NotImplementedException();
    public Task DeleteGoalAsync(int goalId, int userId) => throw new NotImplementedException();
    public Task<GoalScheduleDto> GetScheduleAsync(int goalId, int userId) => throw new NotImplementedException();
    public Task<List<AtRiskCardDto>> GetAtRiskCardsAsync(int goalId, int userId) => throw new NotImplementedException();
    public Task<GoalDto> ExtendGoalAsync(int goalId, int userId, ExtendGoalRequest request) => throw new NotImplementedException();
}
