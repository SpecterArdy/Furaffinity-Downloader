namespace FuraffinityDownloader.Models;

public sealed record DownloadTask(
    Submission Submission,
    User User,
    bool IsThumbnail
);

