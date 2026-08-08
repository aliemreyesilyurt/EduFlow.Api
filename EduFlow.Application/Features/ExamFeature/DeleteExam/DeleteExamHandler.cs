namespace EduFlow.Application.Features.ExamFeature.DeleteExam;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record DeleteExamRequest(Guid Id);

public sealed class DeleteExamHandler(
    IRepository<Exam> examRepository,
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<DeleteExamRequest, Result>
{
    public async Task<Result> HandleAsync(DeleteExamRequest command, CancellationToken cancellationToken)
    {
        var exam = await examRepository.FindAsync(e => e.Id == command.Id, cancellationToken);

        if (exam is null)
        {
            return ExamErrors.NotFound(command.Id);
        }

        var course = await courseRepository.FindAsync(c => c.Id == exam.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanManage(course, tenantContext))
        {
            return ExamErrors.Forbidden;
        }

        await examRepository.DeleteAsync(exam, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
