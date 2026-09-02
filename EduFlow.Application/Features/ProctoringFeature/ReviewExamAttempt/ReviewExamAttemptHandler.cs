namespace EduFlow.Application.Features.ProctoringFeature.ReviewExamAttempt;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Application.Features.ExamFeature;
using EduFlow.Application.Features.PointsFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record ReviewExamAttemptRequest(Guid AttemptId, bool Approved, string? Note);

public sealed record ReviewExamAttemptResponse(Guid Id, bool Approved, DateTime ReviewedOn, string? Note);

public sealed class ReviewExamAttemptHandler(
    IRepository<ExamAttempt> examAttemptRepository,
    IRepository<Exam> examRepository,
    IRepository<Course> courseRepository,
    IPointsAwardService pointsAwardService,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<ReviewExamAttemptRequest, Result<ReviewExamAttemptResponse>>
{
    public async Task<Result<ReviewExamAttemptResponse>> HandleAsync(
        ReviewExamAttemptRequest command, CancellationToken cancellationToken)
    {
        var attempt = await examAttemptRepository.FindAsync(a => a.Id == command.AttemptId, cancellationToken);

        if (attempt is null)
        {
            return ExamErrors.AttemptNotFound(command.AttemptId);
        }

        var exam = await examRepository.FindAsync(e => e.Id == attempt.ExamId, cancellationToken);

        if (exam is null)
        {
            return ExamErrors.AttemptNotFound(command.AttemptId);
        }

        var course = await courseRepository.FindAsync(c => c.Id == exam.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanManage(course, tenantContext))
        {
            return ExamErrors.Forbidden;
        }

        if (attempt.SubmittedOn is null)
        {
            return ProctoringErrors.AttemptNotSubmitted;
        }

        attempt.ReviewApproved = command.Approved;
        attempt.ReviewedBy = tenantContext.UserId;
        attempt.ReviewedOn = DateTime.UtcNow;
        attempt.ReviewNote = command.Note;
        attempt.RequiresReview = false;

        if (command.Approved && attempt.Passed == true && exam.RewardPoints > 0 && !attempt.PointsAwarded)
        {
            await pointsAwardService.AwardExamPointsAsync(attempt, exam, cancellationToken);
        }

        await examAttemptRepository.UpdateAsync(attempt, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new ReviewExamAttemptResponse(
            attempt.Id, attempt.ReviewApproved.Value, attempt.ReviewedOn.Value, attempt.ReviewNote));
    }
}
