namespace EduFlow.Application.Features.CommentFeature.CreateCourseComment;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Application.Features.EnrollmentFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record CreateCourseCommentRequest(Guid CourseId, string Content);

public sealed class CreateCourseCommentHandler(
    IRepository<Course> courseRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Comment> commentRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<CreateCourseCommentRequest, Result<CommentResponse>>
{
    public async Task<Result<CommentResponse>> HandleAsync(CreateCourseCommentRequest command, CancellationToken cancellationToken)
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

        var comment = new Comment
        {
            Id = Guid.CreateVersion7(),
            CourseId = command.CourseId,
            StepId = null,
            AuthorId = studentId,
            Content = command.Content
        };

        await commentRepository.AddAsync(comment, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new CommentResponse(
            comment.Id, comment.CourseId, comment.StepId, comment.AuthorId, comment.Content, comment.IsHidden, comment.CreatedOn));
    }
}
