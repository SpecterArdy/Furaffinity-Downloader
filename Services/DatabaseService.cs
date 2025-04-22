using FuraffinityDownloader.Models;
using Microsoft.Data.Sqlite;

namespace FuraffinityDownloader.Services;

public sealed class DatabaseService
{
    private const string DbPath = "fa_gallery_downloader.sqlite";
    private SqliteConnection? _connection;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        _connection = new SqliteConnection($"Data Source={DbPath};Cache=Shared");
        await _connection.OpenAsync(cancellationToken);
        string userTable = """
            CREATE TABLE IF NOT EXISTS Users (
                Username TEXT PRIMARY KEY,
                AccountName TEXT
            );
        """;
        string subTable = """
            CREATE TABLE IF NOT EXISTS Submissions (
                Id TEXT PRIMARY KEY,
                Title TEXT,
                Username TEXT,
                AccountName TEXT,
                ContentUrl TEXT,
                ContentName TEXT,
                IsScrap INTEGER,
                DateUploaded TEXT
            );
        """;
        await using var userCmd = _connection.CreateCommand();
        userCmd.CommandText = userTable;
        await userCmd.ExecuteNonQueryAsync(cancellationToken);
        await using var subCmd = _connection.CreateCommand();
        subCmd.CommandText = subTable;
        await subCmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveUserAsync(User user, CancellationToken cancellationToken = default)
    {
        if (_connection == null) throw new InvalidOperationException("DB not initialized.");
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Users (Username, AccountName)
            VALUES ($username, $account)
            ON CONFLICT(Username) DO UPDATE SET AccountName=excluded.AccountName;
        """;
        cmd.Parameters.AddWithValue("$username", user.Username);
        cmd.Parameters.AddWithValue("$account", user.AccountName ?? string.Empty);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveSubmissionAsync(Submission submission, CancellationToken cancellationToken = default)
    {
        if (_connection == null) throw new InvalidOperationException("DB not initialized.");
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Submissions (Id, Title, Username, AccountName, ContentUrl, ContentName, IsScrap, DateUploaded)
            VALUES ($id, $title, $username, $account, $url, $name, $scrap, $date)
            ON CONFLICT(Id) DO UPDATE SET 
                Title=excluded.Title,
                Username=excluded.Username,
                AccountName=excluded.AccountName,
                ContentUrl=excluded.ContentUrl,
                ContentName=excluded.ContentName,
                IsScrap=excluded.IsScrap,
                DateUploaded=excluded.DateUploaded;
        """;
        cmd.Parameters.AddWithValue("$id", submission.Id);
        cmd.Parameters.AddWithValue("$title", submission.Title);
        cmd.Parameters.AddWithValue("$username", submission.Username);
        cmd.Parameters.AddWithValue("$account", submission.AccountName ?? string.Empty);
        cmd.Parameters.AddWithValue("$url", submission.ContentUrl);
        cmd.Parameters.AddWithValue("$name", submission.ContentName);
        cmd.Parameters.AddWithValue("$scrap", submission.IsScrap ? 1 : 0);
        cmd.Parameters.AddWithValue("$date", submission.DateUploaded?.ToString("o") ?? string.Empty);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IEnumerable<Submission>> GetSubmissionsForUserAsync(string username, CancellationToken cancellationToken = default)
    {
        if (_connection == null) throw new InvalidOperationException("DB not initialized.");
        await using var cmd = _connection.CreateCommand();
        cmd.CommandText = "SELECT * FROM Submissions WHERE Username = $username;";
        cmd.Parameters.AddWithValue("$username", username);
        var results = new List<Submission>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new Submission(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetBoolean(6),
                reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7))
            ));
        }
        return results;
    }
}

