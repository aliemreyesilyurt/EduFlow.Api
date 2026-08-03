namespace EduFlow.Application.Features.CommentFeature.GetCourseComments;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetCourseCommentsRequest(Guid CourseId);

public sealed record GetCourseCommentsResponse(IReadOnlyList<CommentResponse> Comments);

public sealed class GetCourseCommentsHandler(
    IRepository<Course> courseRepository,
    IRepository<Comment> commentRepository,
    ITenantContext tenantContext) : IHandler<GetCourseCommentsRequest, Result<GetCourseCommentsResponse>>
{
    public async Task<Result<GetCourseCommentsResponse>> HandleAsync(GetCourseCommentsRequest command, CancellationToken cancellationToken)
    {
        var course = await courseRepository.FindAsync(c => c.Id == command.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanView(course, tenantContext))
        {
            return CourseErrors.NotFound(command.CourseId);
        }

        var canModerate = CourseAccess.CanManage(course, tenantContext);

        var comments = (await commentRepository.GetAllAsync(cancellationToken))
            .Where(c => c.CourseId == command.CourseId && c.StepId is null && (canModerate || !c.IsHidden))
            .OrderByDescending(c => c.CreatedOn)
            .Select(c => new CommentResponse(c.Id, c.CourseId, c.StepId, c.AuthorId, c.Content, c.IsHidden, c.CreatedOn))
            .ToList();

        return Result.Success(new GetCourseCommentsResponse(comments));
    }
}
