namespace EduFlow.Application.Features.ExamFeature.UpdateQuestion;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record UpdateQuestionRequest(
    Guid Id,
    string Text,
    int Points,
    IReadOnlyList<QuestionOptionInput> Options);

public sealed class UpdateQuestionHandler(
    IRepository<Question> questionRepository,
    IRepository<Exam> examRepository,
    IRepository<Course> courseRepository,
    IRepository<QuestionOption> optionRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<UpdateQuestionRequest, Result<QuestionResponse>>
{
    public async Task<Result<QuestionResponse>> HandleAsync(UpdateQuestionRequest command, CancellationToken cancellationToken)
    {
        var question = await questionRepository.FindAsync(q => q.Id == command.Id, cancellationToken);

        if (question is null)
        {
            return ExamErrors.QuestionNotFound(command.Id);
        }

        var exam = await examRepository.FindAsync(e => e.Id == question.ExamId, cancellationToken);

        if (exam is null)
        {
            return ExamErrors.QuestionNotFound(command.Id);
        }

        var course = await courseRepository.FindAsync(c => c.Id == exam.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanManage(course, tenantContext))
        {
            return ExamErrors.Forbidden;
        }

        question.Text = command.Text;
        question.Points = command.Points;

        await questionRepository.UpdateAsync(question, cancellationToken);

        // Replace-all strategy: simpler than id-matching a partial update against the submitted
        // option set, and options carry no state (like attempt answers) that would be lost by it.
        var existingOptions = (await optionRepository.GetAllAsync(cancellationToken))
            .Where(o => o.QuestionId == question.Id)
            .ToList();

        optionRepository.RemoveRange(existingOptions);

        var newOptions = command.Options
            .Select((o, i) => new QuestionOption
            {
                Id = Guid.CreateVersion7(),
                QuestionId = question.Id,
                Text = o.Text,
                IsCorrect = o.IsCorrect,
                Order = i + 1
            })
            .ToList();

        await optionRepository.AddRangeAsync(newOptions, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new QuestionResponse(
            question.Id, question.ExamId, question.Text, question.Order, question.Points,
            newOptions.Select(o => new QuestionOptionResponse(o.Id, o.Text, o.Order, o.IsCorrect)).ToList()));
    }
}
