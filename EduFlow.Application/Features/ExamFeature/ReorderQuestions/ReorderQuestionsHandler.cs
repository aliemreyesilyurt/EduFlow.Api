namespace EduFlow.Application.Features.ExamFeature.ReorderQuestions;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record ReorderQuestionsRequest(Guid ExamId, IReadOnlyList<Guid> QuestionIds);

public sealed class ReorderQuestionsHandler(
    IRepository<Exam> examRepository,
    IRepository<Course> courseRepository,
    IRepository<Question> questionRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<ReorderQuestionsRequest, Result>
{
    public async Task<Result> HandleAsync(ReorderQuestionsRequest command, CancellationToken cancellationToken)
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

        var allQuestions = await questionRepository.GetAllAsync(cancellationToken);
        var examQuestions = allQuestions.Where(q => q.ExamId == command.ExamId).ToDictionary(q => q.Id);

        if (examQuestions.Count != command.QuestionIds.Count || !command.QuestionIds.All(examQuestions.ContainsKey))
        {
            return ExamErrors.ReorderMismatch;
        }

        // Same two-phase negative-order trick as ReorderStepsHandler: the (ExamId, Order) unique
        // index would reject any assignment that transiently collides with the current values.
        for (var i = 0; i < command.QuestionIds.Count; i++)
        {
            examQuestions[command.QuestionIds[i]].Order = -(i + 1);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        for (var i = 0; i < command.QuestionIds.Count; i++)
        {
            examQuestions[command.QuestionIds[i]].Order = i + 1;
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
