namespace EduFlow.Application.Features.ProctoringFeature.UploadProctoringSnapshot;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Storage;
using EduFlow.Application.Features.ExamFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record UploadProctoringSnapshotRequest(Guid AttemptId, IFormFile File);

public sealed record UploadProctoringSnapshotResponse(Guid Id, DateTime CapturedOn);

public sealed class UploadProctoringSnapshotHandler(
    IRepository<ExamAttempt> examAttemptRepository,
    IRepository<Exam> examRepository,
    IRepository<ProctoringSnapshot> proctoringSnapshotRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<UploadProctoringSnapshotRequest, Result<UploadProctoringSnapshotResponse>>
{
    public async Task<Result<UploadProctoringSnapshotResponse>> HandleAsync(
        UploadProctoringSnapshotRequest command, CancellationToken cancellationToken)
    {
        var attempt = await examAttemptRepository.FindAsync(a => a.Id == command.AttemptId, cancellationToken);

        if (attempt is null || attempt.StudentId != tenantContext.UserId)
        {
            return ExamErrors.AttemptNotFound(command.AttemptId);
        }

        if (attempt.SubmittedOn is not null)
        {
            return ExamErrors.AttemptAlreadySubmitted;
        }

        if (attempt.ProctoringConsentOn is null)
        {
            return ProctoringErrors.ConsentRequired;
        }

        var exam = await examRepository.FindAsync(e => e.Id == attempt.ExamId, cancellationToken);

        if (exam is null)
        {
            return ExamErrors.AttemptNotFound(command.AttemptId);
        }

        if (!exam.RequireCamera)
        {
            return ProctoringErrors.CameraNotRequired;
        }

        var extension = command.File.ContentType switch
        {
            "image/png" => ".png",
            _ => ".jpg"
        };

        var snapshotId = Guid.CreateVersion7();

        var snapshot = new ProctoringSnapshot
        {
            Id = snapshotId,
            ExamAttemptId = attempt.Id,
            CapturedOn = DateTime.UtcNow,
            FileName = $"{snapshotId}{extension}",
            ContentType = command.File.ContentType,
            SizeBytes = command.File.Length
        };

        await using (var stream = command.File.OpenReadStream())
        {
            await fileStorage.SaveAsync($"proctoring/{attempt.Id}", snapshot.FileName, stream, cancellationToken);
        }

        await proctoringSnapshotRepository.AddAsync(snapshot, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new UploadProctoringSnapshotResponse(snapshot.Id, snapshot.CapturedOn));
    }
}
