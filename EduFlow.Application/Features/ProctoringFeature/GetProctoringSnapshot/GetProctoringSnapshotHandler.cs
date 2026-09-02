namespace EduFlow.Application.Features.ProctoringFeature.GetProctoringSnapshot;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Storage;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Application.Features.ExamFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetProctoringSnapshotRequest(Guid AttemptId, Guid SnapshotId);

public sealed record GetProctoringSnapshotResponse(Stream Content, string ContentType, string FileName);

public sealed class GetProctoringSnapshotHandler(
    IRepository<ExamAttempt> examAttemptRepository,
    IRepository<Exam> examRepository,
    IRepository<Course> courseRepository,
    IRepository<ProctoringSnapshot> proctoringSnapshotRepository,
    IFileStorage fileStorage,
    ITenantContext tenantContext) : IHandler<GetProctoringSnapshotRequest, Result<GetProctoringSnapshotResponse>>
{
    public async Task<Result<GetProctoringSnapshotResponse>> HandleAsync(
        GetProctoringSnapshotRequest command, CancellationToken cancellationToken)
    {
        var attempt = await examAttemptRepository.FindAsync(a => a.Id == command.AttemptId, cancellationToken);

        if (attempt is null)
        {
            return ExamErrors.AttemptNotFound(command.AttemptId);
        }

        var exam = await examRepository.FindAsync(e => e.Id == attempt.ExamId, cancellationToken);

        if (exam is null)
        {
            return ExamErrors.AttemptNotFound(command.AttemptId);
        }

        var course = await courseRepository.FindAsync(c => c.Id == exam.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanManage(course, tenantContext))
        {
            return ExamErrors.AttemptNotFound(command.AttemptId);
        }

        var snapshot = await proctoringSnapshotRepository.FindAsync(
            s => s.Id == command.SnapshotId && s.ExamAttemptId == attempt.Id, cancellationToken);

        if (snapshot is null)
        {
            return ProctoringErrors.SnapshotNotFound(command.SnapshotId);
        }

        var storedFile = await fileStorage.GetAsync($"proctoring/{attempt.Id}", snapshot.FileName, cancellationToken);

        if (storedFile is null)
        {
            return ProctoringErrors.SnapshotNotFound(command.SnapshotId);
        }

        return Result.Success(
            new GetProctoringSnapshotResponse(storedFile.Content, storedFile.ContentType, storedFile.FileName));
    }
}
