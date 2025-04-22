namespace FuraffinityDownloader.Models;

public sealed record Submission(
    string Id,
    string Title,
    string Username,
    string? AccountName,
    string ContentUrl,
    string ContentName,
    bool IsScrap,
    DateTime? DateUploaded
);

