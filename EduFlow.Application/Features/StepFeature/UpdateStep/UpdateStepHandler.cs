namespace EduFlow.Application.Features.StepFeature.UpdateStep;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record UpdateStepRequest(
    Guid Id,
    string Title,
    StepContentType ContentType,
    string? ContentUrl,
    string? TextContent);

public sealed record UpdateStepResponse(
    Guid Id,
    Guid CourseId,
    string Title,
    int Order,
    StepContentType ContentType,
    string? ContentUrl,
    string? TextContent);

public sealed class UpdateStepHandler(
    IRepository<Step> stepRepository,
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<UpdateStepRequest, Result<UpdateStepResponse>>
{
    public async Task<Result<UpdateStepResponse>> HandleAsync(UpdateStepRequest command, CancellationToken cancellationToken)
    {
        var step = await stepRepository.FindAsync(s => s.Id == command.Id, cancellationToken);

        if (step is null)
        {
            return StepErrors.NotFound(command.Id);
        }

        var course = await courseRepository.FindAsync(c => c.Id == step.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanManage(course, tenantContext))
        {
            return StepErrors.Forbidden;
        }

        step.Title = command.Title;
        step.ContentType = command.ContentType;
        step.ContentUrl = command.ContentUrl;
        step.TextContent = command.TextContent;

        await stepRepository.UpdateAsync(step, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new UpdateStepResponse(
            step.Id, step.CourseId, step.Title, step.Order, step.ContentType, step.ContentUrl, step.TextContent));
    }
}
