using FinanceManager.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace FinanceManager.Infrastructure.Services;

public class LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger) : IFileStorageService
{
    private readonly string _basePath = Path.Combine(env.ContentRootPath, "uploads");

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder = "documents", CancellationToken cancellationToken = default)
    {
        var folderPath = Path.Combine(_basePath, folder);
        Directory.CreateDirectory(folderPath);

        var safeFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(folderPath, safeFileName);

        await using var fs = File.Create(filePath);
        await stream.CopyToAsync(fs, cancellationToken);

        logger.LogInformation("File uploaded: {Path}", filePath);
        return Path.Combine(folder, safeFileName).Replace('\\', '/');
    }

    public Task<Stream?> DownloadAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, path);
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, path);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public string GetPublicUrl(string path) => $"/uploads/{path.Replace('\\', '/')}";
}
