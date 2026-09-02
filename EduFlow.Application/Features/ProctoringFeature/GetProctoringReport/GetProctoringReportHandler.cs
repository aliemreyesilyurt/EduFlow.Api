namespace EduFlow.Application.Features.ProctoringFeature.GetProctoringReport;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Identity;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Application.Features.ExamFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record GetProctoringReportRequest(Guid AttemptId);

public sealed record ProctoringEventSummary(Guid Id, ProctoringEventType Type, DateTime OccurredOn, string? Details);

public sealed record ProctoringSnapshotSummary(Guid Id, DateTime CapturedOn);

public sealed record GetProctoringReportResponse(
    Guid AttemptId,
    Guid StudentId,
    string StudentName,
    int ViolationCount,
    bool RequiresReview,
    bool? ReviewApproved,
    DateTime? ReviewedOn,
    string? ReviewNote,
    IReadOnlyList<ProctoringEventSummary> Events,
    IReadOnlyList<ProctoringSnapshotSummary> Snapshots);

public sealed class GetProctoringReportHandler(
    IRepository<ExamAttempt> examAttemptRepository,
    IRepository<Exam> examRepository,
    IRepository<Course> courseRepository,
    IRepository<ProctoringEvent> proctoringEventRepository,
    IRepository<ProctoringSnapshot> proctoringSnapshotRepository,
    IIdentityService identityService,
    ITenantContext tenantContext) : IHandler<GetProctoringReportRequest, Result<GetProctoringReportResponse>>
{
    public async Task<Result<GetProctoringReportResponse>> HandleAsync(
        GetProctoringReportRequest command, CancellationToken cancellationToken)
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

        var events = (await proctoringEventRepository.GetAllAsync(cancellationToken))
            .Where(e => e.ExamAttemptId == attempt.Id)
            .OrderBy(e => e.OccurredOn)
            .Select(e => new ProctoringEventSummary(e.Id, e.Type, e.OccurredOn, e.Details))
            .ToList();

        var snapshots = (await proctoringSnapshotRepository.GetAllAsync(cancellationToken))
            .Where(s => s.ExamAttemptId == attempt.Id)
            .OrderBy(s => s.CapturedOn)
            .Select(s => new ProctoringSnapshotSummary(s.Id, s.CapturedOn))
            .ToList();

        var studentNames = await identityService.GetDisplayNamesAsync([attempt.StudentId], cancellationToken);

        return Result.Success(new GetProctoringReportResponse(
            attempt.Id, attempt.StudentId, studentNames.GetValueOrDefault(attempt.StudentId, "Unknown"),
            attempt.ViolationCount, attempt.RequiresReview, attempt.ReviewApproved, attempt.ReviewedOn, attempt.ReviewNote,
            events, snapshots));
    }
}
