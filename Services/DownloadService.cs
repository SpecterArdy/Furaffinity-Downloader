using FuraffinityDownloader.Models;

using FuraffinityDownloader.Models;

namespace FuraffinityDownloader.Services;

public sealed class DownloadService
{
    private readonly HttpClient _httpClient = new();

    public async Task DownloadAsync(Submission submission, string outputRoot, CancellationToken cancellationToken = default)
    {
        // Create per-user directory
        var userFolder = submission.Username.ToLowerInvariant();
        var dir = Path.Combine(outputRoot, userFolder);
        Directory.CreateDirectory(dir);

        var fileUrl = submission.ContentUrl;
        var originalName = submission.ContentName;
        var baseName = Path.GetFileNameWithoutExtension(originalName);
        var extension = Path.GetExtension(originalName);
        var safeBaseName = FuraffinityDownloader.Utilities.PathUtils.SanitizeFilename(baseName);
        var safeName = safeBaseName + extension;
        if (!string.Equals(baseName, safeBaseName, StringComparison.Ordinal))
            Console.WriteLine($"[WARN] Filename sanitized: '{baseName}' -> '{safeBaseName}' (extension '{extension}')");

        var filePath = Path.Combine(dir, safeName);

        try
        {
            // Download file to path
            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            await response.Content.CopyToAsync(fileStream, cancellationToken);
            Console.WriteLine($"[DOWNLOADED] {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAIL] Download failed for {filePath}: {ex.Message}");
        }
    }
    }
