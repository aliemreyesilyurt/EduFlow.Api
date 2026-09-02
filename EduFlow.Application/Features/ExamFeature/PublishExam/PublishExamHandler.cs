namespace EduFlow.Application.Features.ExamFeature.PublishExam;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record PublishExamRequest(Guid Id);

public sealed class PublishExamHandler(
    IRepository<Exam> examRepository,
    IRepository<Course> courseRepository,
    IRepository<Question> questionRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<PublishExamRequest, Result<ExamSummaryResponse>>
{
    public async Task<Result<ExamSummaryResponse>> HandleAsync(PublishExamRequest command, CancellationToken cancellationToken)
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

        if (exam.IsPublished)
        {
            return ExamErrors.AlreadyPublished;
        }

        var hasQuestions = (await questionRepository.GetAllAsync(cancellationToken))
            .Any(q => q.ExamId == exam.Id);

        if (!hasQuestions)
        {
            return ExamErrors.NoQuestions;
        }

        exam.IsPublished = true;

        await examRepository.UpdateAsync(exam, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new ExamSummaryResponse(
            exam.Id, exam.CourseId, exam.Title, exam.PassScorePercentage,
            exam.TimeLimitMinutes, exam.MaxAttempts, exam.IsPublished,
            exam.ProctoringEnabled, exam.RequireCamera, exam.SnapshotIntervalSeconds, exam.ViolationWarningThreshold,
            exam.RewardPoints));
    }
}
