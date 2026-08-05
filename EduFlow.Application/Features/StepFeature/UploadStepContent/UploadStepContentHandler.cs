namespace EduFlow.Application.Features.StepFeature.UploadStepContent;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Storage;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record UploadStepContentRequest(Guid StepId, IFormFile File);

public sealed record UploadStepContentResponse(Guid Id, string ContentUrl, string FileName, long SizeBytes);

public sealed class UploadStepContentHandler(
    IRepository<Step> stepRepository,
    IRepository<Course> courseRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<UploadStepContentRequest, Result<UploadStepContentResponse>>
{
    public async Task<Result<UploadStepContentResponse>> HandleAsync(UploadStepContentRequest command, CancellationToken cancellationToken)
    {
        var step = await stepRepository.FindAsync(s => s.Id == command.StepId, cancellationToken);

        if (step is null)
        {
            return StepErrors.NotFound(command.StepId);
        }

        var course = await courseRepository.FindAsync(c => c.Id == step.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanManage(course, tenantContext))
        {
            return StepErrors.Forbidden;
        }

        var extension = Path.GetExtension(command.File.FileName);

        if (!StepContentFileTypes.MatchesContentType(extension, step.ContentType))
        {
            return StepErrors.ContentTypeMismatch;
        }

        var directory = $"steps/{step.Id}";
        await fileStorage.DeleteDirectoryAsync(directory, cancellationToken);

        await using (var stream = command.File.OpenReadStream())
        {
            await fileStorage.SaveAsync(directory, command.File.FileName, stream, cancellationToken);
        }

        step.ContentUrl = $"steps/{step.Id}/content";

        await stepRepository.UpdateAsync(step, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(
            new UploadStepContentResponse(step.Id, step.ContentUrl, command.File.FileName, command.File.Length));
    }
}
