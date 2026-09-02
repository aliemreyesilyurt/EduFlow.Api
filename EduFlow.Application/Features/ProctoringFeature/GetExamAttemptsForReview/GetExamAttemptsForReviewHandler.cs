namespace EduFlow.Application.Features.ProctoringFeature.GetExamAttemptsForReview;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Application.Features.ExamFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetExamAttemptsForReviewRequest(Guid CourseId);

public sealed record ExamAttemptReviewSummary(
    Guid Id,
    Guid StudentId,
    string StudentName,
    int AttemptNumber,
    DateTime StartedOn,
    DateTime? SubmittedOn,
    double? ScorePercentage,
    bool? Passed,
    int ViolationCount,
    bool RequiresReview,
    bool? ReviewApproved);

public sealed record GetExamAttemptsForReviewResponse(IReadOnlyList<ExamAttemptReviewSummary> Attempts);

public sealed class GetExamAttemptsForReviewHandler(
    IRepository<Course> courseRepository,
    IRepository<Exam> examRepository,
    IRepository<ExamAttempt> examAttemptRepository,
    IIdentityService identityService,
    ITenantContext tenantContext) : IHandler<GetExamAttemptsForReviewRequest, Result<GetExamAttemptsForReviewResponse>>
{
    public async Task<Result<GetExamAttemptsForReviewResponse>> HandleAsync(
        GetExamAttemptsForReviewRequest command, CancellationToken cancellationToken)
    {
        var course = await courseRepository.FindAsync(c => c.Id == command.CourseId, cancellationToken);

        if (course is null)
        {
            return CourseErrors.NotFound(command.CourseId);
        }

        if (!CourseAccess.CanManage(course, tenantContext))
        {
            return CourseErrors.Forbidden;
        }

        var exam = await examRepository.FindAsync(e => e.CourseId == command.CourseId, cancellationToken);

        if (exam is null)
        {
            return Result.Success(new GetExamAttemptsForReviewResponse([]));
        }

        var attempts = (await examAttemptRepository.GetAllAsync(cancellationToken))
            .Where(a => a.ExamId == exam.Id)
            .OrderByDescending(a => a.StartedOn)
            .ToList();

        var studentNames = await identityService.GetDisplayNamesAsync(
            attempts.Select(a => a.StudentId), cancellationToken);

        var summaries = attempts
            .Select(a => new ExamAttemptReviewSummary(
                a.Id, a.StudentId, studentNames.GetValueOrDefault(a.StudentId, "Unknown"),
                a.AttemptNumber, a.StartedOn, a.SubmittedOn, a.ScorePercentage, a.Passed,
                a.ViolationCount, a.RequiresReview, a.ReviewApproved))
            .ToList();

        return Result.Success(new GetExamAttemptsForReviewResponse(summaries));
    }
}
