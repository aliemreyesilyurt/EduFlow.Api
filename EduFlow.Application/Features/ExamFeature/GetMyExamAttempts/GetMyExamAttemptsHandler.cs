namespace EduFlow.Application.Features.ExamFeature.GetMyExamAttempts;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetMyExamAttemptsRequest(Guid CourseId);

public sealed record ExamAttemptSummary(
    Guid Id,
    int AttemptNumber,
    DateTime StartedOn,
    DateTime? SubmittedOn,
    double? ScorePercentage,
    bool? Passed,
    int ViolationCount,
    bool RequiresReview,
    bool? ReviewApproved);

public sealed record GetMyExamAttemptsResponse(IReadOnlyList<ExamAttemptSummary> Attempts);

public sealed class GetMyExamAttemptsHandler(
    IRepository<Course> courseRepository,
    IRepository<Exam> examRepository,
    IRepository<ExamAttempt> examAttemptRepository,
    ITenantContext tenantContext) : IHandler<GetMyExamAttemptsRequest, Result<GetMyExamAttemptsResponse>>
{
    public async Task<Result<GetMyExamAttemptsResponse>> HandleAsync(GetMyExamAttemptsRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not { } studentId)
        {
            return Result.Success(new GetMyExamAttemptsResponse([]));
        }

        var course = await courseRepository.FindAsync(c => c.Id == command.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanView(course, tenantContext))
        {
            return CourseErrors.NotFound(command.CourseId);
        }

        var exam = await examRepository.FindAsync(e => e.CourseId == command.CourseId, cancellationToken);

        if (exam is null)
        {
            return Result.Success(new GetMyExamAttemptsResponse([]));
        }

        var attempts = (await examAttemptRepository.GetAllAsync(cancellationToken))
            .Where(a => a.ExamId == exam.Id && a.StudentId == studentId)
            .OrderByDescending(a => a.StartedOn)
            .Select(a => new ExamAttemptSummary(
                a.Id, a.AttemptNumber, a.StartedOn, a.SubmittedOn, a.ScorePercentage, a.Passed,
                a.ViolationCount, a.RequiresReview, a.ReviewApproved))
            .ToList();

        return Result.Success(new GetMyExamAttemptsResponse(attempts));
    }
}
