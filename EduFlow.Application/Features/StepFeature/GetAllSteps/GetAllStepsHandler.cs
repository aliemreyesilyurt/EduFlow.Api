namespace EduFlow.Application.Features.StepFeature.GetAllSteps;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record GetAllStepsRequest(Guid CourseId);

public sealed record StepSummary(
    Guid Id,
    Guid CourseId,
    string Title,
    int Order,
    StepContentType ContentType,
    string? ContentUrl,
    string? TextContent);

public sealed record GetAllStepsResponse(IReadOnlyList<StepSummary> Steps);

public sealed class GetAllStepsHandler(
    IRepository<Course> courseRepository,
    IRepository<Step> stepRepository,
    ITenantContext tenantContext) : IHandler<GetAllStepsRequest, Result<GetAllStepsResponse>>
{
    public async Task<Result<GetAllStepsResponse>> HandleAsync(GetAllStepsRequest command, CancellationToken cancellationToken)
    {
        var course = await courseRepository.FindAsync(c => c.Id == command.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanView(course, tenantContext))
        {
            return CourseErrors.NotFound(command.CourseId);
        }

        var steps = await stepRepository.GetAllAsync(cancellationToken);

        var ordered = steps
            .Where(s => s.CourseId == command.CourseId)
            .OrderBy(s => s.Order)
            .Select(s => new StepSummary(s.Id, s.CourseId, s.Title, s.Order, s.ContentType, s.ContentUrl, s.TextContent))
            .ToList();

        return Result.Success(new GetAllStepsResponse(ordered));
    }
}
