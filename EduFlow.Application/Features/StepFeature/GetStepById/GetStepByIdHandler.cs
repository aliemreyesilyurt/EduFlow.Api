namespace EduFlow.Application.Features.StepFeature.GetStepById;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record GetStepByIdRequest(Guid Id);

public sealed record GetStepByIdResponse(
    Guid Id,
    Guid CourseId,
    string Title,
    int Order,
    StepContentType ContentType,
    string? ContentUrl,
    string? TextContent);

public sealed class GetStepByIdHandler(
    IRepository<Step> stepRepository,
    IRepository<Course> courseRepository,
    ITenantContext tenantContext) : IHandler<GetStepByIdRequest, Result<GetStepByIdResponse>>
{
    public async Task<Result<GetStepByIdResponse>> HandleAsync(GetStepByIdRequest command, CancellationToken cancellationToken)
    {
        var step = await stepRepository.FindAsync(s => s.Id == command.Id, cancellationToken);

        if (step is null)
        {
            return StepErrors.NotFound(command.Id);
        }

        var course = await courseRepository.FindAsync(c => c.Id == step.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanView(course, tenantContext))
        {
            return StepErrors.NotFound(command.Id);
        }

        return Result.Success(new GetStepByIdResponse(
            step.Id, step.CourseId, step.Title, step.Order, step.ContentType, step.ContentUrl, step.TextContent));
    }
}
