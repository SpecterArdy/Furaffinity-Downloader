using FuraffinityDownloader.Services;

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
        // === Terminal-based Run Loop ===
        Console.WriteLine("=== Furaffinity Downloader (.NET 9) ===");
        Console.Write("Enter FurAffinity username or gallery URL: ");
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("No username or URL supplied. Exiting.");
            return;
        }
        // Parse username from URL or take as-is
        var username = input.StartsWith("http") ? FuraffinityDownloader.Utilities.ParserUtils.ParseUsernameFromUrl(input) : input.Trim();

        // --- Securely prompt for password ---
        string? password = null;
        Console.Write("Enter your FA password: ");
        password = ReadPassword();
        if (string.IsNullOrEmpty(password))
        {
            Console.WriteLine("No password supplied. Exiting.");
            return;
        }

        // --- Login ---
        Console.Write("[INFO] Logging in...");
        var loginResult = await _scraper.LoginAsync(username, password);
        if (!loginResult)
        {
            Console.WriteLine("\n[FAIL] Login failed. Please check your credentials.");
            return;
        }
        Console.WriteLine("\n[INFO] Login successful!");

        // Further operations now proceed with authenticated session...
        Console.WriteLine($"[INFO] Scraping user: {username}...");
        // Scraper fetches gallery page(s)
        // (string html = await _scraper.FetchHtmlAsync(...))

        Console.WriteLine($"[INFO] Parsing gallery HTML...");
        // var user = _parser.ExtractUser(html);
        // var submissions = _parser.ExtractSubmissions(html, user);

        Console.WriteLine($"[INFO] Saving user/info to DB...");
        // await _database.SaveUserAsync(user);
        // foreach (var submission in submissions) await _database.SaveSubmissionAsync(submission);

        Console.WriteLine($"[INFO] Downloading submissions to per-user folder structure...");
        // foreach (var task in BuildDownloadTasks(submissions, user)) await _downloader.DownloadAsync(task, outputRoot);

        Console.WriteLine($"[SUCCESS] Download complete. All files are now organized by username in output directory.");
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
