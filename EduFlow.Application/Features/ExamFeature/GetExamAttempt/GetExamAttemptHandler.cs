namespace EduFlow.Application.Features.ExamFeature.GetExamAttempt;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetExamAttemptRequest(Guid Id);

public sealed class GetExamAttemptHandler(
    IRepository<ExamAttempt> examAttemptRepository,
    IRepository<Exam> examRepository,
    IRepository<Course> courseRepository,
    IRepository<Question> questionRepository,
    IRepository<QuestionOption> optionRepository,
    IRepository<ExamAnswer> examAnswerRepository,
    ITenantContext tenantContext) : IHandler<GetExamAttemptRequest, Result<ExamAttemptResponse>>
{
    public async Task<Result<ExamAttemptResponse>> HandleAsync(GetExamAttemptRequest command, CancellationToken cancellationToken)
    {
        var attempt = await examAttemptRepository.FindAsync(a => a.Id == command.Id, cancellationToken);

        if (attempt is null)
        {
            return ExamErrors.AttemptNotFound(command.Id);
        }

        var exam = await examRepository.FindAsync(e => e.Id == attempt.ExamId, cancellationToken);

        if (exam is null)
        {
            return ExamErrors.AttemptNotFound(command.Id);
        }

        var course = await courseRepository.FindAsync(c => c.Id == exam.CourseId, cancellationToken);
        var isOwner = attempt.StudentId == tenantContext.UserId;
        var isManager = course is not null && CourseAccess.CanManage(course, tenantContext);

        if (!isOwner && !isManager)
        {
            return ExamErrors.AttemptNotFound(command.Id);
        }

        if (attempt.SubmittedOn is null)
        {
            return Result.Success(new ExamAttemptResponse(
                attempt.Id, attempt.ExamId, attempt.AttemptNumber, attempt.StartedOn, exam.TimeLimitMinutes,
                null, null, null, null,
                attempt.ViolationCount, attempt.RequiresReview, attempt.ReviewApproved, attempt.ReviewedOn, attempt.ReviewNote,
                attempt.PointsAwarded, null));
        }

        var questions = (await questionRepository.GetAllAsync(cancellationToken))
            .Where(q => q.ExamId == exam.Id)
            .ToDictionary(q => q.Id);

        var correctOptionByQuestion = (await optionRepository.GetAllAsync(cancellationToken))
            .Where(o => o.IsCorrect && questions.ContainsKey(o.QuestionId))
            .ToDictionary(o => o.QuestionId, o => o.Id);

        var answers = (await examAnswerRepository.GetAllAsync(cancellationToken))
            .Where(a => a.ExamAttemptId == attempt.Id)
            .Select(a =>
            {
                var question = questions[a.QuestionId];
                var correctOptionId = correctOptionByQuestion.GetValueOrDefault(a.QuestionId);

                return new ExamAnswerResult(
                    a.QuestionId, question.Text, a.SelectedOptionId, correctOptionId,
                    a.SelectedOptionId is not null && a.SelectedOptionId == correctOptionId);
            })
            .ToList();

        return Result.Success(new ExamAttemptResponse(
            attempt.Id, attempt.ExamId, attempt.AttemptNumber, attempt.StartedOn, exam.TimeLimitMinutes,
            attempt.SubmittedOn, attempt.ScorePercentage, exam.PassScorePercentage, attempt.Passed,
            attempt.ViolationCount, attempt.RequiresReview, attempt.ReviewApproved, attempt.ReviewedOn, attempt.ReviewNote,
            attempt.PointsAwarded, answers));
    }
}
