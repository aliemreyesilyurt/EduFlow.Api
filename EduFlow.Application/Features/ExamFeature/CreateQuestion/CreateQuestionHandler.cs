namespace EduFlow.Application.Features.ExamFeature.CreateQuestion;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record CreateQuestionRequest(
    Guid ExamId,
    string Text,
    int Points,
    IReadOnlyList<QuestionOptionInput> Options);

public sealed class CreateQuestionHandler(
    IRepository<Exam> examRepository,
    IRepository<Course> courseRepository,
    IRepository<Question> questionRepository,
    IRepository<QuestionOption> optionRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<CreateQuestionRequest, Result<QuestionResponse>>
{
    public async Task<Result<QuestionResponse>> HandleAsync(CreateQuestionRequest command, CancellationToken cancellationToken)
    {
        var exam = await examRepository.FindAsync(e => e.Id == command.ExamId, cancellationToken);

        if (exam is null)
        {
            return ExamErrors.NotFound(command.ExamId);
        }

        var course = await courseRepository.FindAsync(c => c.Id == exam.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanManage(course, tenantContext))
        {
            return ExamErrors.Forbidden;
        }

        var existingQuestions = (await questionRepository.GetAllAsync(cancellationToken))
            .Where(q => q.ExamId == command.ExamId)
            .ToList();
        var nextOrder = existingQuestions.Count == 0 ? 1 : existingQuestions.Max(q => q.Order) + 1;

        var question = new Question
        {
            Id = Guid.CreateVersion7(),
            ExamId = command.ExamId,
            Text = command.Text,
            Order = nextOrder,
            Points = command.Points
        };

        await questionRepository.AddAsync(question, cancellationToken);

        var options = command.Options
            .Select((o, i) => new QuestionOption
            {
                Id = Guid.CreateVersion7(),
                QuestionId = question.Id,
                Text = o.Text,
                IsCorrect = o.IsCorrect,
                Order = i + 1
            })
            .ToList();

        await optionRepository.AddRangeAsync(options, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new QuestionResponse(
            question.Id, question.ExamId, question.Text, question.Order, question.Points,
            options.Select(o => new QuestionOptionResponse(o.Id, o.Text, o.Order, o.IsCorrect)).ToList()));
    }
}
