namespace EduFlow.Application.Options;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public required string RootPath { get; set; }

    public long MaxFileSizeBytes { get; set; } = 524_288_000;
}
