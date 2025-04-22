namespace FuraffinityDownloader.Utilities;

public static class ParserUtils
{
    public static string ParseUsernameFromUrl(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        var segments = input.TrimEnd('/').Split('/');
        var idx = Array.IndexOf(segments, "user");
        if (idx != -1 && idx + 1 < segments.Length)
            return segments[idx + 1];
        return input.Trim();
    }
}

