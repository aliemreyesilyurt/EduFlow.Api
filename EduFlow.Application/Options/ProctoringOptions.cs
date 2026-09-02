namespace EduFlow.Application.Options;

public sealed class ProctoringOptions
{
    public const string SectionName = "Proctoring";

    public long MaxSnapshotSizeBytes { get; set; } = 2 * 1024 * 1024;

    public string[] AllowedSnapshotContentTypes { get; set; } = ["image/jpeg", "image/png"];

    public int DefaultRetentionDays { get; set; } = 30;

    public int MaxEventsPerAttempt { get; set; } = 500;

    public required string DefaultConsentText { get; set; }
}
