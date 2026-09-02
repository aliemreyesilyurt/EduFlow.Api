namespace EduFlow.Application.Features.ExamFeature.UnpublishExam;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record UnpublishExamRequest(Guid Id);

public sealed class UnpublishExamHandler(
    IRepository<Exam> examRepository,
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<UnpublishExamRequest, Result<ExamSummaryResponse>>
{
    public async Task<Result<ExamSummaryResponse>> HandleAsync(UnpublishExamRequest command, CancellationToken cancellationToken)
    {
        var exam = await examRepository.FindAsync(e => e.Id == command.Id, cancellationToken);

        if (exam is null)
        {
            return ExamErrors.NotFound(command.Id);
        }

        var course = await courseRepository.FindAsync(c => c.Id == exam.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanManage(course, tenantContext))
        {
            return ExamErrors.Forbidden;
        }

        if (!exam.IsPublished)
        {
            return ExamErrors.NotPublished;
        }

        exam.IsPublished = false;

        await examRepository.UpdateAsync(exam, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new ExamSummaryResponse(
            exam.Id, exam.CourseId, exam.Title, exam.PassScorePercentage,
            exam.TimeLimitMinutes, exam.MaxAttempts, exam.IsPublished,
            exam.ProctoringEnabled, exam.RequireCamera, exam.SnapshotIntervalSeconds, exam.ViolationWarningThreshold,
            exam.RewardPoints));
    }
}
