namespace EduFlow.Application.Features.ExamFeature.UpdateExam;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record UpdateExamRequest(
    Guid Id,
    string Title,
    int PassScorePercentage,
    int? TimeLimitMinutes,
    int? MaxAttempts);

public sealed class UpdateExamHandler(
    IRepository<Exam> examRepository,
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<UpdateExamRequest, Result<ExamSummaryResponse>>
{
    public async Task<Result<ExamSummaryResponse>> HandleAsync(UpdateExamRequest command, CancellationToken cancellationToken)
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

        exam.Title = command.Title;
        exam.PassScorePercentage = command.PassScorePercentage;
        exam.TimeLimitMinutes = command.TimeLimitMinutes;
        exam.MaxAttempts = command.MaxAttempts;

        await examRepository.UpdateAsync(exam, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new ExamSummaryResponse(
            exam.Id, exam.CourseId, exam.Title, exam.PassScorePercentage,
            exam.TimeLimitMinutes, exam.MaxAttempts, exam.IsPublished));
    }
}
