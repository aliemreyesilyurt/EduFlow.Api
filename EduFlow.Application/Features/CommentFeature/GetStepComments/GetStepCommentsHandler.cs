namespace EduFlow.Application.Features.CommentFeature.GetStepComments;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Application.Features.StepFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetStepCommentsRequest(Guid StepId);

public sealed record GetStepCommentsResponse(IReadOnlyList<CommentResponse> Comments);

public sealed class GetStepCommentsHandler(
    IRepository<Step> stepRepository,
    IRepository<Course> courseRepository,
    IRepository<Comment> commentRepository,
    IIdentityService identityService,
    ITenantContext tenantContext) : IHandler<GetStepCommentsRequest, Result<GetStepCommentsResponse>>
{
    public async Task<Result<GetStepCommentsResponse>> HandleAsync(GetStepCommentsRequest command, CancellationToken cancellationToken)
    {
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

        var canModerate = CourseAccess.CanManage(course, tenantContext);

        var matchingComments = (await commentRepository.GetAllAsync(cancellationToken))
            .Where(c => c.StepId == command.StepId && (canModerate || !c.IsHidden))
            .OrderByDescending(c => c.CreatedOn)
            .ToList();

        var authorNames = await identityService.GetDisplayNamesAsync(
            matchingComments.Select(c => c.AuthorId), cancellationToken);

        var comments = matchingComments
            .Select(c => new CommentResponse(
                c.Id, c.CourseId, c.StepId, c.AuthorId, authorNames.GetValueOrDefault(c.AuthorId, "Unknown"),
                c.Content, c.IsHidden, c.CreatedOn))
            .ToList();

        return Result.Success(new GetStepCommentsResponse(comments));
    }
}
