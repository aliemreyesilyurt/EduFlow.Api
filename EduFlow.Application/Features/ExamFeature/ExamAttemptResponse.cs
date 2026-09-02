namespace EduFlow.Application.Features.ExamFeature;

public sealed record ExamAnswerResult(
    Guid QuestionId,
    string QuestionText,
    Guid? SelectedOptionId,
    Guid? CorrectOptionId,
    bool IsCorrect);

public sealed record ExamAttemptResponse(
    Guid Id,
    Guid ExamId,
    int AttemptNumber,
    DateTime StartedOn,
    int? TimeLimitMinutes,
    DateTime? SubmittedOn,
    double? ScorePercentage,
    int? PassScorePercentage,
    bool? Passed,
    int ViolationCount,
    bool RequiresReview,
    bool? ReviewApproved,
    DateTime? ReviewedOn,
    string? ReviewNote,
    IReadOnlyList<ExamAnswerResult>? Answers);
