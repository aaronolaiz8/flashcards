using Retainica.Api.DTOs.Study;

namespace Retainica.Api.Services.Interfaces;

public interface IStudyService
{
    Task<SessionDto> StartSessionAsync(int userId, StartSessionRequest request);
    Task<ReviewResultDto> SubmitReviewAsync(int sessionId, int userId, SubmitReviewRequest request);
    Task<SessionDto> EndSessionAsync(int sessionId, int userId);
}
