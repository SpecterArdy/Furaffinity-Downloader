using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace FuraffinityDownloader.Services;

public sealed class ScraperService
{
    private readonly HttpClient _httpClient;
    private bool _isAuthenticated;

    public ScraperService()
    {
        // Use a CookieContainer for session management (required for FA)
        var handler = new HttpClientHandler
        {
            CookieContainer = new CookieContainer(),
            AllowAutoRedirect = true,
            UseCookies = true
        };
        _httpClient = new HttpClient(handler, disposeHandler: true);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; FADownloader.NET/1.0)");
    }

    public async Task<bool> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var loginUrl = "https://www.furaffinity.net/login/";
        var postData = new Dictionary<string, string>
        {
            { "action", "login" },
            { "name", username },
            { "pass", password },
            { "login", "Login to FurAffinity" }
        };
        using var content = new FormUrlEncodedContent(postData);
        using var response = await _httpClient.PostAsync(loginUrl, content, cancellationToken);
        // FurAffinity doesn't use true HTTP status for failed login, so check for redirect to homepage
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var success = response.IsSuccessStatusCode && !responseBody.Contains("The password you entered was incorrect") && !responseBody.Contains("Secure login");
        _isAuthenticated = success;
        return success;
    }

    public async Task<string> FetchHtmlAsync(string url, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}

