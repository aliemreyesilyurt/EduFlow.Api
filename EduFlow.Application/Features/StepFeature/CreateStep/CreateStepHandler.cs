namespace EduFlow.Application.Features.StepFeature.CreateStep;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record CreateStepRequest(
    Guid CourseId,
    string Title,
    StepContentType ContentType,
    string? ContentUrl,
    string? TextContent);

public sealed record CreateStepResponse(
    Guid Id,
    Guid CourseId,
    string Title,
    int Order,
    StepContentType ContentType,
    string? ContentUrl,
    string? TextContent);

public sealed class CreateStepHandler(
    IRepository<Course> courseRepository,
    IRepository<Step> stepRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<CreateStepRequest, Result<CreateStepResponse>>
{
    public async Task<Result<CreateStepResponse>> HandleAsync(CreateStepRequest command, CancellationToken cancellationToken)
    {
        var course = await courseRepository.FindAsync(c => c.Id == command.CourseId, cancellationToken);

        if (course is null)
        {
            return CourseErrors.NotFound(command.CourseId);
        }

        if (!CourseAccess.CanManage(course, tenantContext))
        {
            return CourseErrors.Forbidden;
        }

        var allSteps = await stepRepository.GetAllAsync(cancellationToken);
        var courseSteps = allSteps.Where(s => s.CourseId == command.CourseId).ToList();
        var nextOrder = courseSteps.Count == 0 ? 1 : courseSteps.Max(s => s.Order) + 1;

        var step = new Step
        {
            Id = Guid.CreateVersion7(),
            CourseId = command.CourseId,
            Title = command.Title,
            Order = nextOrder,
            ContentType = command.ContentType,
            ContentUrl = command.ContentUrl,
            TextContent = command.TextContent
        };

        await stepRepository.AddAsync(step, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new CreateStepResponse(
            step.Id, step.CourseId, step.Title, step.Order, step.ContentType, step.ContentUrl, step.TextContent));
    }
}
