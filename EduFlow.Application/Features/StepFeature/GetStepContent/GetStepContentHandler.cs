namespace EduFlow.Application.Features.StepFeature.GetStepContent;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Storage;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetStepContentRequest(Guid Id);

public sealed record GetStepContentResponse(Stream Content, string ContentType, string FileName);

public sealed class GetStepContentHandler(
    IRepository<Step> stepRepository,
    IRepository<Course> courseRepository,
    IFileStorage fileStorage,
    ITenantContext tenantContext) : IHandler<GetStepContentRequest, Result<GetStepContentResponse>>
{
    public async Task<Result<GetStepContentResponse>> HandleAsync(GetStepContentRequest command, CancellationToken cancellationToken)
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

        var storedFile = await fileStorage.GetAsync($"steps/{step.Id}", cancellationToken);

        if (storedFile is null)
        {
            return StepErrors.NotFound(command.Id);
        }

        return Result.Success(
            new GetStepContentResponse(storedFile.Content, storedFile.ContentType, storedFile.FileName));
    }
}
