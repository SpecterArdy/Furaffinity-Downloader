using FuraffinityDownloader.Models;

namespace FuraffinityDownloader.Services;

public sealed class DatabaseService
{
    public DatabaseService()
    {
        // SQLite connection/init logic
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // Create tables if necessary
        throw new NotImplementedException();
    }

    public async Task SaveUserAsync(User user, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task SaveSubmissionAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Submission>> GetSubmissionsForUserAsync(string username, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

