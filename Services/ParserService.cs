using FuraffinityDownloader.Models;
using HtmlAgilityPack;

namespace FuraffinityDownloader.Services;

public sealed class ParserService
{
    public ParserService() { }

    public User? ExtractUser(string html, string? fallbackUsername = null, string? galleryUrl = null)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        // Try original XPath
        var usernameNode = doc.DocumentNode.SelectSingleNode("//section[@id='userpage-nav']//a[contains(@href, '/user/')]");
        var username = usernameNode?.InnerText.Trim().ToLowerInvariant();
        if (!string.IsNullOrEmpty(username))
        {
            Console.WriteLine("[PARSE] Username parsed from HTML DOM.");
            return new User(username, null);
        }
        // Fallback to provided username from App.cs
        if (!string.IsNullOrEmpty(fallbackUsername))
        {
            Console.WriteLine("[PARSE] Username fallback: using input arg.");
            return new User(fallbackUsername.ToLowerInvariant(), null);
        }
        // Fallback to /user/ or /gallery/ in galleryUrl
        if (!string.IsNullOrEmpty(galleryUrl))
        {
            var parsed = FuraffinityDownloader.Utilities.ParserUtils.ParseUsernameFromUrl(galleryUrl);
            if (!string.IsNullOrEmpty(parsed))
            {
                Console.WriteLine("[PARSE] Username fallback: extracted from gallery URL.");
                return new User(parsed.ToLowerInvariant(), null);
            }
        }
        Console.WriteLine("[PARSE] Failed to extract username by any method.");
        return null;
    }

    public IEnumerable<Submission> ExtractSubmissions(string html, User user)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        // Submissions are usually in .gallery .submission or .section-gallery-content > figure
        var galleryNodes = doc.DocumentNode.SelectNodes("//figure[contains(@class, 't-image') or contains(@class,'t-image-gallery')]");
        if (galleryNodes is null)
            yield break;
        foreach (var node in galleryNodes)
        {
            var linkNode = node.SelectSingleNode(".//a[contains(@href, '/view/')]");
            var titleNode = node.SelectSingleNode(".//figcaption") ?? node.SelectSingleNode(".//img");
            var url = linkNode?.Attributes["href"]?.Value ?? "";
            if (!string.IsNullOrEmpty(url))
            {
                // e.g., /view/12345678/ -> 12345678
                var id = url.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? Guid.NewGuid().ToString();
                var contentName = titleNode?.InnerText.Trim() ?? id;
                yield return new Submission(
                    id,
                    contentName,
                    user.Username,
                    user.AccountName,
                    url.StartsWith("http") ? url : $"https://www.furaffinity.net{url}",
                    contentName,
                    false,
                    null
                );
            }
        }
    }
}

