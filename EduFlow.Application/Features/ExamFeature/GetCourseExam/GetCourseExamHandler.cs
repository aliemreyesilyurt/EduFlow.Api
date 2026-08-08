namespace EduFlow.Application.Features.ExamFeature.GetCourseExam;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetCourseExamRequest(Guid CourseId);

public sealed record ExamDetailResponse(
    Guid Id,
    Guid CourseId,
    string Title,
    int PassScorePercentage,
    int? TimeLimitMinutes,
    int? MaxAttempts,
    bool IsPublished,
    IReadOnlyList<QuestionResponse> Questions);

public sealed class GetCourseExamHandler(
    IRepository<Course> courseRepository,
    IRepository<Exam> examRepository,
    IRepository<Question> questionRepository,
    IRepository<QuestionOption> optionRepository,
    ITenantContext tenantContext) : IHandler<GetCourseExamRequest, Result<ExamDetailResponse>>
{
    public async Task<Result<ExamDetailResponse>> HandleAsync(GetCourseExamRequest command, CancellationToken cancellationToken)
    {
        var course = await courseRepository.FindAsync(c => c.Id == command.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanManage(course, tenantContext))
        {
            return CourseErrors.NotFound(command.CourseId);
        }

        var exam = await examRepository.FindAsync(e => e.CourseId == command.CourseId, cancellationToken);

        if (exam is null)
        {
            return ExamErrors.NotFound(command.CourseId);
        }

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
                    .Select(o => new QuestionOptionResponse(o.Id, o.Text, o.Order, o.IsCorrect))
                    .ToList()))
            .ToList();

        return Result.Success(new ExamDetailResponse(
            exam.Id, exam.CourseId, exam.Title, exam.PassScorePercentage,
            exam.TimeLimitMinutes, exam.MaxAttempts, exam.IsPublished, questionResponses));
    }
}
