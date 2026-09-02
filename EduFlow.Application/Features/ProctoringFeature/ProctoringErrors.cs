using EduFlow.Domain.Abstractions.Errors;

namespace EduFlow.Application.Features.ProctoringFeature;

public static class ProctoringErrors
{
    public static readonly Error ConsentRequired =
        Error.Forbidden("Proctoring.ConsentRequired", "You must give proctoring consent before this action");

    public static readonly Error CameraNotRequired =
        Error.Conflict("Proctoring.CameraNotRequired", "This exam does not require camera snapshots");

    public static Error SnapshotNotFound(Guid id) =>
        Error.NotFound("Proctoring.SnapshotNotFound", $"The snapshot with Id '{id}' was not found");

    public static readonly Error AttemptNotSubmitted =
        Error.Conflict("Proctoring.AttemptNotSubmitted", "The exam attempt must be submitted before it can be reviewed");
}
