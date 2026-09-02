namespace EduFlow.Application.Features.ExamFeature.SubmitExamAttempt;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.PointsFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record SubmitExamAnswerInput(Guid QuestionId, Guid? SelectedOptionId);

public sealed record SubmitExamAttemptRequest(Guid AttemptId, IReadOnlyList<SubmitExamAnswerInput> Answers);

public sealed class SubmitExamAttemptHandler(
    IRepository<ExamAttempt> examAttemptRepository,
    IRepository<Exam> examRepository,
    IRepository<Question> questionRepository,
    IRepository<QuestionOption> optionRepository,
    IRepository<ExamAnswer> examAnswerRepository,
    IPointsAwardService pointsAwardService,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<SubmitExamAttemptRequest, Result<ExamAttemptResponse>>
{
    public async Task<Result<ExamAttemptResponse>> HandleAsync(SubmitExamAttemptRequest command, CancellationToken cancellationToken)
    {
        var attempt = await examAttemptRepository.FindAsync(a => a.Id == command.AttemptId, cancellationToken);

        if (attempt is null || attempt.StudentId != tenantContext.UserId)
        {
            return ExamErrors.AttemptNotFound(command.AttemptId);
        }

        if (attempt.SubmittedOn is not null)
        {
            return ExamErrors.AttemptAlreadySubmitted;
        }

        var exam = await examRepository.FindAsync(e => e.Id == attempt.ExamId, cancellationToken);

        if (exam is null)
        {
            return ExamErrors.AttemptNotFound(command.AttemptId);
        }

        var questions = (await questionRepository.GetAllAsync(cancellationToken))
            .Where(q => q.ExamId == exam.Id)
            .ToList();

        var options = (await optionRepository.GetAllAsync(cancellationToken))
            .Where(o => questions.Select(q => q.Id).Contains(o.QuestionId))
            .ToLookup(o => o.QuestionId);

        var submittedAnswers = command.Answers.ToDictionary(a => a.QuestionId, a => a.SelectedOptionId);

        var results = new List<ExamAnswerResult>();
        var examAnswers = new List<ExamAnswer>();
        var earnedPoints = 0;
        var totalPoints = 0;

        foreach (var question in questions)
        {
            totalPoints += question.Points;

            var correctOption = options[question.Id].FirstOrDefault(o => o.IsCorrect);
            var selectedOptionId = submittedAnswers.GetValueOrDefault(question.Id);
            var isCorrect = selectedOptionId is not null && correctOption is not null && selectedOptionId == correctOption.Id;

            if (isCorrect)
            {
                earnedPoints += question.Points;
            }

            examAnswers.Add(new ExamAnswer
            {
                Id = Guid.CreateVersion7(),
                ExamAttemptId = attempt.Id,
                QuestionId = question.Id,
                SelectedOptionId = selectedOptionId
            });

            results.Add(new ExamAnswerResult(
                question.Id, question.Text, selectedOptionId, correctOption?.Id, isCorrect));
        }

        var scorePercentage = totalPoints == 0 ? 0d : Math.Round(100.0 * earnedPoints / totalPoints, 2);
        var passed = scorePercentage >= exam.PassScorePercentage;

        await examAnswerRepository.AddRangeAsync(examAnswers, cancellationToken);

        attempt.SubmittedOn = DateTime.UtcNow;
        attempt.ScorePercentage = scorePercentage;
        attempt.Passed = passed;

        // A review-required attempt only earns points once ReviewExamAttemptHandler approves it;
        // otherwise (no proctoring, or no violations serious enough to require review) passing is
        // itself sufficient, so the reward is credited immediately.
        if (passed && exam.RewardPoints > 0 && !attempt.RequiresReview && !attempt.PointsAwarded)
        {
            await pointsAwardService.AwardExamPointsAsync(attempt, exam, cancellationToken);
        }

        await examAttemptRepository.UpdateAsync(attempt, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new ExamAttemptResponse(
            attempt.Id, attempt.ExamId, attempt.AttemptNumber, attempt.StartedOn, exam.TimeLimitMinutes,
            attempt.SubmittedOn, attempt.ScorePercentage, exam.PassScorePercentage, attempt.Passed,
            attempt.ViolationCount, attempt.RequiresReview, attempt.ReviewApproved, attempt.ReviewedOn, attempt.ReviewNote,
            attempt.PointsAwarded, results));
    }
}
