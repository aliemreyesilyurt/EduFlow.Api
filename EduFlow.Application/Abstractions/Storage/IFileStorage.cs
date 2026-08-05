namespace EduFlow.Application.Abstractions.Storage;

public sealed record StoredFile(Stream Content, string ContentType, string FileName);

public interface IFileStorage
{
    Task SaveAsync(string relativeDirectory, string fileName, Stream content, CancellationToken cancellationToken = default);

    Task<StoredFile?> GetAsync(string relativeDirectory, CancellationToken cancellationToken = default);

    Task DeleteDirectoryAsync(string relativeDirectory, CancellationToken cancellationToken = default);
}
