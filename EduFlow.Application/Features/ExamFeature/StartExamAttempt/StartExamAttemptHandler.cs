namespace EduFlow.Application.Features.ExamFeature.StartExamAttempt;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Application.Features.EnrollmentFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record StartExamAttemptRequest(Guid CourseId);

public sealed class StartExamAttemptHandler(
    IRepository<Course> courseRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Exam> examRepository,
    IRepository<ExamAttempt> examAttemptRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<StartExamAttemptRequest, Result<ExamAttemptResponse>>
{
    public async Task<Result<ExamAttemptResponse>> HandleAsync(StartExamAttemptRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not { } studentId)
        {
            return EnrollmentErrors.NotEnrolled;
        }

        var course = await courseRepository.FindAsync(c => c.Id == command.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanView(course, tenantContext))
        {
            return CourseErrors.NotFound(command.CourseId);
        }

        var enrollment = await enrollmentRepository.FindAsync(
            e => e.CourseId == command.CourseId && e.StudentId == studentId, cancellationToken);

        if (enrollment is null)
        {
            return EnrollmentErrors.NotEnrolled;
        }

        if (enrollment.CompletedOn is null)
        {
            return ExamErrors.CourseNotCompleted;
        }

        var exam = await examRepository.FindAsync(e => e.CourseId == command.CourseId, cancellationToken);

        if (exam is null || !exam.IsPublished)
        {
            return ExamErrors.NotFound(command.CourseId);
        }

        var attempts = (await examAttemptRepository.GetAllAsync(cancellationToken))
            .Where(a => a.ExamId == exam.Id && a.StudentId == studentId)
            .ToList();

        var inProgress = attempts.FirstOrDefault(a => a.SubmittedOn is null);

        if (inProgress is not null)
        {
            return Result.Success(ToResponse(inProgress, exam));
        }

        var submittedCount = attempts.Count(a => a.SubmittedOn is not null);

        if (exam.MaxAttempts is { } max && submittedCount >= max)
        {
            return ExamErrors.AttemptLimitReached;
        }

        var attempt = new ExamAttempt
        {
            Id = Guid.CreateVersion7(),
            ExamId = exam.Id,
            StudentId = studentId,
            AttemptNumber = submittedCount + 1,
            StartedOn = DateTime.UtcNow,
            SubmittedOn = null
        };

        await examAttemptRepository.AddAsync(attempt, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(ToResponse(attempt, exam));
    }

    private static ExamAttemptResponse ToResponse(ExamAttempt attempt, Exam exam) => new(
        attempt.Id, attempt.ExamId, attempt.AttemptNumber, attempt.StartedOn, exam.TimeLimitMinutes,
        attempt.SubmittedOn, attempt.ScorePercentage, null, attempt.Passed, null);
}
