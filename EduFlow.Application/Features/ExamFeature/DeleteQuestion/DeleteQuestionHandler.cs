namespace EduFlow.Application.Features.ExamFeature.DeleteQuestion;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record DeleteQuestionRequest(Guid Id);

public sealed class DeleteQuestionHandler(
    IRepository<Question> questionRepository,
    IRepository<Exam> examRepository,
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<DeleteQuestionRequest, Result>
{
    public async Task<Result> HandleAsync(DeleteQuestionRequest command, CancellationToken cancellationToken)
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

        await questionRepository.DeleteAsync(question, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
