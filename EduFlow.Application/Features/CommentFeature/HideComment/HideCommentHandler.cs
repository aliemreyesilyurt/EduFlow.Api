namespace EduFlow.Application.Features.CommentFeature.HideComment;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record HideCommentRequest(Guid CommentId);

public sealed class HideCommentHandler(
    IRepository<Comment> commentRepository,
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    IIdentityService identityService,
    ITenantContext tenantContext) : IHandler<HideCommentRequest, Result<CommentResponse>>
{
    public async Task<Result<CommentResponse>> HandleAsync(HideCommentRequest command, CancellationToken cancellationToken)
    {
        var comment = await commentRepository.FindAsync(c => c.Id == command.CommentId, cancellationToken);

        if (comment is null)
        {
            return CommentErrors.NotFound(command.CommentId);
        }

        var course = await courseRepository.FindAsync(c => c.Id == comment.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanManage(course, tenantContext))
        {
            return CommentErrors.Forbidden;
        }

        comment.IsHidden = true;

        await commentRepository.UpdateAsync(comment, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        var authorNames = await identityService.GetDisplayNamesAsync([comment.AuthorId], cancellationToken);

        return Result.Success(new CommentResponse(
            comment.Id, comment.CourseId, comment.StepId, comment.AuthorId,
            authorNames.GetValueOrDefault(comment.AuthorId, "Unknown"),
            comment.Content, comment.IsHidden, comment.CreatedOn));
    }
}
