using FlashcardsApp.Api.Data;
using FlashcardsApp.Api.DTOs.Study;
using FlashcardsApp.Api.Services.Interfaces;

namespace FlashcardsApp.Api.Services;

public class StudyService(AppDbContext db, IFsrsService fsrs) : IStudyService
{
    public Task<SessionDto> StartSessionAsync(int userId, StartSessionRequest request) => throw new NotImplementedException();
    public Task<ReviewResultDto> SubmitReviewAsync(int sessionId, int userId, SubmitReviewRequest request) => throw new NotImplementedException();
    public Task<SessionDto> EndSessionAsync(int sessionId, int userId) => throw new NotImplementedException();
}
