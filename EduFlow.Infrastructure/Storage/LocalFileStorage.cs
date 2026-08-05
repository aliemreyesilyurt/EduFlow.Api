using EduFlow.Application.Abstractions.Storage;
using EduFlow.Application.Options;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace EduFlow.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    private readonly string _rootPath;

    public LocalFileStorage(IHostEnvironment environment, IOptions<StorageOptions> options)
    {
        _rootPath = Path.IsPathRooted(options.Value.RootPath)
            ? options.Value.RootPath
            : Path.Combine(environment.ContentRootPath, options.Value.RootPath);
    }

    public async Task SaveAsync(
        string relativeDirectory,
        string fileName,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(_rootPath, relativeDirectory);
        Directory.CreateDirectory(directory);

        var safeFileName = Path.GetFileName(fileName);
        var filePath = Path.Combine(directory, safeFileName);

        await using var fileStream = new FileStream(
            filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
        await content.CopyToAsync(fileStream, cancellationToken);
    }

    public Task<StoredFile?> GetAsync(string relativeDirectory, CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(_rootPath, relativeDirectory);

        if (!Directory.Exists(directory))
        {
            return Task.FromResult<StoredFile?>(null);
        }

        var filePath = Directory.EnumerateFiles(directory).FirstOrDefault();
        if (filePath is null)
        {
            return Task.FromResult<StoredFile?>(null);
        }

        if (!ContentTypeProvider.TryGetContentType(filePath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        Stream stream = new FileStream(
            filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);

        return Task.FromResult<StoredFile?>(new StoredFile(stream, contentType, Path.GetFileName(filePath)));
    }

    public Task DeleteDirectoryAsync(string relativeDirectory, CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(_rootPath, relativeDirectory);

        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        return Task.CompletedTask;
    }
}
