using FuraffinityDownloader.Services;
using FuraffinityDownloader.Models;

namespace FuraffinityDownloader;

public sealed class App
{
    private readonly ScraperService _scraper;
    private readonly ParserService _parser;
    private readonly DownloadService _downloader;
    private readonly DatabaseService _database;

    public App()
    {
        _scraper = new ScraperService();
        _parser = new ParserService();
        _database = new DatabaseService();
        _downloader = new DownloadService();
    }

    public async Task RunAsync()
    {
        Console.WriteLine("=== Furaffinity Downloader (.NET 9) ===");
        // Prompt FIRST for login username/password
        Console.Write("Enter your FA login username: ");
        var loginUsername = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(loginUsername))
        {
            Console.WriteLine("No login username supplied. Exiting.");
            return;
        }
        Console.Write("Enter your FA password: ");
        var loginPassword = ReadPassword();
        if (string.IsNullOrWhiteSpace(loginPassword))
        {
            Console.WriteLine("No password supplied. Exiting.");
            return;
        }
        Console.Write("[INFO] Logging in...");
        var loginResult = await _scraper.LoginAsync(loginUsername, loginPassword);
        if (!loginResult)
        {
            Console.WriteLine("\n[FAIL] Login failed. Please check your credentials.");
            return;
        }
        Console.WriteLine("\n[INFO] Login successful!");

        // Prompt for whom to scrape
        Console.Write("\nEnter FurAffinity username or gallery URL to scrape: ");
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("No username or URL supplied. Exiting.");
            return;
        }
        var username = input.StartsWith("http") ? FuraffinityDownloader.Utilities.ParserUtils.ParseUsernameFromUrl(input) : input.Trim();

        Console.WriteLine($"[INFO] Scraping user: {username}...");
        // Scraper fetches gallery page(s)
        string html;
        try
        {
            html = await _scraper.FetchHtmlAsync($"https://www.furaffinity.net/gallery/{username}/");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[FAIL] Unable to fetch gallery page: {ex.Message}");
            return;
        }

        Console.WriteLine($"[INFO] Parsing gallery HTML...");
        var user = _parser.ExtractUser(html, username, $"https://www.furaffinity.net/gallery/{username}/");
        if (user == null)
        {
            Console.WriteLine("[FAIL] Unable to extract user from HTML or fallback methods.");
            return;
        }

        var submissions = _parser.ExtractSubmissions(html, user).ToList();
        Console.WriteLine($"[INFO] Parsed {submissions.Count} submissions for user {user.Username}.");
        foreach (var s in submissions)
            Console.WriteLine($"    - {s.Id}: {s.ContentUrl} ({s.ContentName})");

        // --- Save to DB ---
        try
        {
            await _database.InitializeAsync();
            await _database.SaveUserAsync(user);
            foreach (var sub in submissions)
                await _database.SaveSubmissionAsync(sub);
            Console.WriteLine($"[INFO] Saved user and submissions to SQLite database.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAIL] Database error: {ex.Message}");
        }

        // --- Download files ---
        // Root output in ./Furaffinity
        var baseRoot = Path.Combine(Directory.GetCurrentDirectory(), "Furaffinity");
        Directory.CreateDirectory(baseRoot);
        Console.WriteLine($"[INFO] Downloading submissions to {baseRoot}\\{user.Username}...");

        // ---- Scrape ALL GALLERY & SCRAPS pages before download ----
        var allGalleryHtmls = await _scraper.FetchAllPaginatedPagesAsync($"https://www.furaffinity.net/gallery/{username}/");
        var allScrapsHtmls = await _scraper.FetchAllPaginatedPagesAsync($"https://www.furaffinity.net/scraps/{username}/");
        var allSubmissions = new List<Submission>();
        foreach (var htmlPage in allGalleryHtmls.Concat(allScrapsHtmls))
            allSubmissions.AddRange(_parser.ExtractSubmissions(htmlPage, user));
        var deduped = allSubmissions.DistinctBy(s => s.Id).ToList();
        Console.WriteLine($"[INFO] Total unique submissions (gallery+scraps): {deduped.Count}.");

        foreach (var sub in deduped)
        {
            // Fetch actual submission page, extract media URL + real filename
            string subHtml;
            try
            {
                subHtml = await _scraper.FetchHtmlAsync(sub.ContentUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] Could not fetch submission page {sub.ContentUrl}: {ex.Message}");
                continue;
            }
            var (mediaUrl, realFilename) = _parser.ExtractDownloadUrlFromSubmissionHtml(subHtml);
            if (string.IsNullOrEmpty(mediaUrl) || string.IsNullOrEmpty(realFilename))
            {
                Console.WriteLine($"[FAIL] Could not parse media/file in {sub.ContentUrl}");
                continue;
            }
            // Download file, preserve extension
            await _downloader.DownloadAsync(
                sub with { ContentUrl = mediaUrl, ContentName = realFilename },
                baseRoot // always download to root "Furaffinity"
            );
        }

        Console.WriteLine($"[SUCCESS] Complete. All files downloaded and indexed.");
    }

    private static string ReadPassword()
    {
        // Mask password input in console
        var pwd = string.Empty;
        ConsoleKeyInfo key;
        do
        {
            key = Console.ReadKey(true);
            if (key.Key is ConsoleKey.Backspace && pwd.Length > 0)
            {
                pwd = pwd[..^1];
                Console.Write("\b \b");
            }
            else if (!char.IsControl(key.KeyChar))
            {
                pwd += key.KeyChar;
                Console.Write('*');
            }
        } while (key.Key != ConsoleKey.Enter);
        Console.WriteLine();
        return pwd;
    }
}
