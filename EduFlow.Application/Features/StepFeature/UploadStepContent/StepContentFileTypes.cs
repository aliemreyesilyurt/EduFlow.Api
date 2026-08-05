namespace EduFlow.Application.Features.StepFeature.UploadStepContent;

using EduFlow.Domain.Enums;

internal static class StepContentFileTypes
{
    private static readonly HashSet<string> VideoExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".webm", ".mov" };

    private static readonly HashSet<string> DocumentExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".pdf", ".doc", ".docx", ".ppt", ".pptx" };

    public static bool IsAllowed(string extension) =>
        VideoExtensions.Contains(extension) || DocumentExtensions.Contains(extension);

    public static bool MatchesContentType(string extension, StepContentType contentType) => contentType switch
    {
        StepContentType.Video => VideoExtensions.Contains(extension),
        StepContentType.Document => DocumentExtensions.Contains(extension),
        _ => false
    };
}
