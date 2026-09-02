namespace EduFlow.Application.Features.ExamFeature;

public sealed record ExamSummaryResponse(
    Guid Id,
    Guid CourseId,
    string Title,
    int PassScorePercentage,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    bool IsPublished,
    bool ProctoringEnabled,
    bool RequireCamera,
    int? SnapshotIntervalSeconds,
    int? ViolationWarningThreshold);
