using FuraffinityDownloader.Models;

namespace FuraffinityDownloader.Services;

public sealed class ParserService
{
    public ParserService()
    {
    }

    public User? ExtractUser(string html)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Submission> ExtractSubmissions(string html, User user)
    {
        throw new NotImplementedException();
    }
}

