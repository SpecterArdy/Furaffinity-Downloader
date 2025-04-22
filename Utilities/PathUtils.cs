namespace FuraffinityDownloader.Utilities;

public static class PathUtils
{
    public static string SanitizeFilename(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "unnamed";
        // Remove forbidden chars \ / : * ? " < > | and control chars.
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(input.Where(ch => !invalid.Contains(ch) && !char.IsControl(ch)).ToArray());
        // Replace whitespace blocks with underscores.
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", "_");
        // Strip leading/trailing dots/spaces.
        cleaned = cleaned.Trim('.', ' ');
        // If empty after cleaning, use fallback.
        return string.IsNullOrEmpty(cleaned) ? "unnamed" : cleaned;
    }
}

