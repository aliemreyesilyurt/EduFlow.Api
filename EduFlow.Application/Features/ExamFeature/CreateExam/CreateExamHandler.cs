namespace EduFlow.Application.Features.ExamFeature.CreateExam;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record CreateExamRequest(
    Guid CourseId,
    string Title,
    int PassScorePercentage,
    int? TimeLimitMinutes,
    int? MaxAttempts);

public sealed class CreateExamHandler(
    IRepository<Course> courseRepository,
    IRepository<Exam> examRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<CreateExamRequest, Result<ExamSummaryResponse>>
{
    public async Task<Result<ExamSummaryResponse>> HandleAsync(CreateExamRequest command, CancellationToken cancellationToken)
    {
        var course = await courseRepository.FindAsync(c => c.Id == command.CourseId, cancellationToken);

        if (course is null)
        {
            return CourseErrors.NotFound(command.CourseId);
        }

        if (!CourseAccess.CanManage(course, tenantContext))
        {
            return ExamErrors.Forbidden;
        }

        var existingExam = await examRepository.FindAsync(e => e.CourseId == command.CourseId, cancellationToken);

        if (existingExam is not null)
        {
            return ExamErrors.AlreadyExists;
        }

        var exam = new Exam
        {
            Id = Guid.CreateVersion7(),
            CourseId = command.CourseId,
            Title = command.Title,
            PassScorePercentage = command.PassScorePercentage,
            TimeLimitMinutes = command.TimeLimitMinutes,
            MaxAttempts = command.MaxAttempts,
            IsPublished = false
        };

        await examRepository.AddAsync(exam, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new ExamSummaryResponse(
            exam.Id, exam.CourseId, exam.Title, exam.PassScorePercentage,
            exam.TimeLimitMinutes, exam.MaxAttempts, exam.IsPublished));
    }
}
