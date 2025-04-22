using FuraffinityDownloader.Models;

namespace FuraffinityDownloader.Services;

public sealed class DownloadService
{
    public DownloadService()
    {
    }

    public async Task DownloadAsync(DownloadTask task, string outputRoot, CancellationToken cancellationToken = default)
    {
        // Download the file and organize into username subfolder under outputRoot
        throw new NotImplementedException();
    }
}

