namespace EduFlow.Application.Features.CommentFeature.CreateStepComment;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Application.Features.EnrollmentFeature;
using EduFlow.Application.Features.StepFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record CreateStepCommentRequest(Guid StepId, string Content);

public sealed class CreateStepCommentHandler(
    IRepository<Step> stepRepository,
    IRepository<Course> courseRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Comment> commentRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<CreateStepCommentRequest, Result<CommentResponse>>
{
    public async Task<Result<CommentResponse>> HandleAsync(CreateStepCommentRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not { } studentId)
        {
            return EnrollmentErrors.NotEnrolled;
        }

        var step = await stepRepository.FindAsync(s => s.Id == command.StepId, cancellationToken);

        if (step is null)
        {
            return StepErrors.NotFound(command.StepId);
        }

        var course = await courseRepository.FindAsync(c => c.Id == step.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanView(course, tenantContext))
        {
            return StepErrors.NotFound(command.StepId);
        }

        var enrollment = await enrollmentRepository.FindAsync(
            e => e.CourseId == step.CourseId && e.StudentId == studentId, cancellationToken);

        if (enrollment is null)
        {
            return EnrollmentErrors.NotEnrolled;
        }

        var comment = new Comment
        {
            Id = Guid.CreateVersion7(),
            CourseId = step.CourseId,
            StepId = step.Id,
            AuthorId = studentId,
            Content = command.Content
        };

        await commentRepository.AddAsync(comment, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new CommentResponse(
            comment.Id, comment.CourseId, comment.StepId, comment.AuthorId, comment.Content, comment.IsHidden, comment.CreatedOn));
    }
}
