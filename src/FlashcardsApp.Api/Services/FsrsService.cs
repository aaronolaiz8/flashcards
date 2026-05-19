using FlashcardsApp.Api.Models;
using FlashcardsApp.Api.Services.Interfaces;

namespace FlashcardsApp.Api.Services;

public class FsrsService : IFsrsService
{
    // FSRS-4.5 default weights
    private static readonly float[] W = [
        0.4072f, 1.1829f, 3.1262f, 15.4722f, 7.2102f, 0.5316f, 1.0651f, 0.0589f,
        1.5330f, 0.1544f, 1.0070f, 1.9395f, 0.1100f, 0.2900f, 2.2700f, 0.2400f, 2.9898f
    ];

    private const float TargetRetentionGood = 0.9f;
    private const float TargetRetentionHard = 0.85f;
    private const float TargetRetentionEasy = 0.95f;

    // Learning steps in minutes: [1, 10], graduating = 1 day, easy = 4 days
    private static readonly int[] LearningStepMinutes = [1, 10];
    private const int GraduatingIntervalDays = 1;
    private const int EasyIntervalDays = 4;
    private const int RelearningStepMinutes = 10;

    public CardMemoryState ProcessReview(CardMemoryState state, int rating, DateTime reviewedAt)
    {
        state.LastRating = rating;
        state.LastReviewedAt = reviewedAt;
        state.ReviewCount++;

        switch (state.State)
        {
            case CardState.New:
                state.Difficulty = InitDifficulty(rating);
                ProcessNewCard(state, rating, reviewedAt);
                break;

            case CardState.Learning:
                ProcessLearningCard(state, rating, reviewedAt);
                break;

            case CardState.Review:
                ProcessReviewCard(state, rating, reviewedAt);
                break;

            case CardState.Relearning:
                ProcessRelearningCard(state, rating, reviewedAt);
                break;
        }

        return state;
    }

    public float ComputeRetrievability(CardMemoryState state, DateTime asOf)
    {
        if (state.Stability == null || state.LastReviewedAt == null)
            return 0f;

        float daysSinceReview = (float)(asOf - state.LastReviewedAt.Value).TotalDays;
        if (daysSinceReview <= 0) return 1f;

        return MathF.Pow(0.9f, daysSinceReview / state.Stability.Value);
    }

    public List<CardMemoryState> GetDueCards(IEnumerable<CardMemoryState> states, int? goalId, DateTime asOf)
    {
        return states
            .Where(s => s.State == CardState.New || s.NextReviewDate <= asOf)
            .OrderBy(s => s.State == CardState.New ? 1 : 0)
            .ThenBy(s => s.NextReviewDate ?? DateTime.MinValue)
            .ToList();
    }

    private void ProcessNewCard(CardMemoryState state, int rating, DateTime reviewedAt)
    {
        if (rating == 4) // Easy — skip learning entirely
        {
            state.Stability = W[0] * EasyIntervalDays;
            state.State = CardState.Review;
            state.NextReviewDate = reviewedAt.AddDays(EasyIntervalDays);
            return;
        }

        state.State = CardState.Learning;
        state.LearningStep = 0;
        AdvanceLearningStep(state, rating, reviewedAt);
    }

    private void ProcessLearningCard(CardMemoryState state, int rating, DateTime reviewedAt)
    {
        if (rating == 1) // Again — restart
        {
            state.LearningStep = 0;
            state.NextReviewDate = reviewedAt.AddMinutes(LearningStepMinutes[0]);
            return;
        }

        if (rating == 4) // Easy — graduate immediately
        {
            Graduate(state, reviewedAt, easy: true);
            return;
        }

        AdvanceLearningStep(state, rating, reviewedAt);
    }

    private void ProcessReviewCard(CardMemoryState state, int rating, DateTime reviewedAt)
    {
        float r = ComputeRetrievability(state, reviewedAt);
        float s = state.Stability ?? 1f;
        float d = state.Difficulty ?? 5f;

        if (rating == 1) // Again — lapse
        {
            state.Stability = MathF.Max(0.1f, s * 0.2f);
            state.State = CardState.Relearning;
            state.LearningStep = 0;
            state.NextReviewDate = reviewedAt.AddMinutes(RelearningStepMinutes);
            return;
        }

        float targetR = rating switch
        {
            2 => TargetRetentionHard,
            3 => TargetRetentionGood,
            4 => TargetRetentionEasy,
            _ => TargetRetentionGood
        };

        float newS = ComputeNewStability(s, d, r, rating);
        float intervalDays = MathF.Max(1f, newS * MathF.Log(targetR) / MathF.Log(0.9f));

        state.Stability = newS;
        state.NextReviewDate = reviewedAt.AddDays(intervalDays);
    }

    private void ProcessRelearningCard(CardMemoryState state, int rating, DateTime reviewedAt)
    {
        if (rating == 1)
        {
            state.NextReviewDate = reviewedAt.AddMinutes(RelearningStepMinutes);
            return;
        }

        // Pass relearning — re-enter Review with current stability
        state.State = CardState.Review;
        float s = state.Stability ?? 1f;
        state.NextReviewDate = reviewedAt.AddDays(MathF.Max(1f, s));
    }

    private void AdvanceLearningStep(CardMemoryState state, int rating, DateTime reviewedAt)
    {
        int nextStep = state.LearningStep + 1;

        if (nextStep >= LearningStepMinutes.Length)
        {
            Graduate(state, reviewedAt, easy: false);
            return;
        }

        state.LearningStep = nextStep;
        int delayMinutes = rating == 2
            ? LearningStepMinutes[state.LearningStep - 1] // Hard — repeat current step
            : LearningStepMinutes[state.LearningStep];

        state.NextReviewDate = reviewedAt.AddMinutes(delayMinutes);
    }

    private void Graduate(CardMemoryState state, DateTime reviewedAt, bool easy)
    {
        int days = easy ? EasyIntervalDays : GraduatingIntervalDays;
        state.Stability = W[0] * days;
        state.State = CardState.Review;
        state.LearningStep = 0;
        state.NextReviewDate = reviewedAt.AddDays(days);
    }

    private float InitDifficulty(int rating) => rating switch
    {
        1 => 9f,
        2 => 7f,
        3 => 5f,
        4 => 3f,
        _ => 5f
    };

    private float ComputeNewStability(float s, float d, float r, int rating)
    {
        float ratingFactor = rating switch
        {
            2 => -0.15f,
            3 => 0f,
            4 => 0.15f,
            _ => 0f
        };

        float modifier = MathF.Exp(W[8] * (11f - d) * MathF.Pow(r, W[9]) *
            (MathF.Exp(W[10] * (1f - ratingFactor)) - 1f));

        return MathF.Max(0.1f, s * modifier);
    }
}
