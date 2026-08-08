namespace EduFlow.Application.Features.ExamFeature.GetExamForTaking;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Application.Features.EnrollmentFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetExamForTakingRequest(Guid CourseId);

public sealed record ExamTakingResponse(
    Guid ExamId,
    string Title,
    int PassScorePercentage,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    int AttemptsUsed,
    int? AttemptsRemaining,
    Guid? InProgressAttemptId,
    IReadOnlyList<QuestionResponse> Questions);

public sealed class GetExamForTakingHandler(
    IRepository<Course> courseRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Exam> examRepository,
    IRepository<Question> questionRepository,
    IRepository<QuestionOption> optionRepository,
    IRepository<ExamAttempt> examAttemptRepository,
    ITenantContext tenantContext) : IHandler<GetExamForTakingRequest, Result<ExamTakingResponse>>
{
    public async Task<Result<ExamTakingResponse>> HandleAsync(GetExamForTakingRequest command, CancellationToken cancellationToken)
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

        var attemptsUsed = attempts.Count(a => a.SubmittedOn is not null);
        var inProgress = attempts.FirstOrDefault(a => a.SubmittedOn is null);
        var attemptsRemaining = exam.MaxAttempts is { } max ? Math.Max(0, max - attemptsUsed) : (int?)null;

        var questions = (await questionRepository.GetAllAsync(cancellationToken))
            .Where(q => q.ExamId == exam.Id)
            .OrderBy(q => q.Order)
            .ToList();

        var options = (await optionRepository.GetAllAsync(cancellationToken))
            .Where(o => questions.Select(q => q.Id).Contains(o.QuestionId))
            .ToLookup(o => o.QuestionId);

        var questionResponses = questions
            .Select(q => new QuestionResponse(
                q.Id, q.ExamId, q.Text, q.Order, q.Points,
                options[q.Id]
                    .OrderBy(o => o.Order)
                    .Select(o => new QuestionOptionResponse(o.Id, o.Text, o.Order, IsCorrect: null))
                    .ToList()))
            .ToList();

        return Result.Success(new ExamTakingResponse(
            exam.Id, exam.Title, exam.PassScorePercentage, exam.TimeLimitMinutes, exam.MaxAttempts,
            attemptsUsed, attemptsRemaining, inProgress?.Id, questionResponses));
    }
}
