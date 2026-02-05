using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;
using CloudStream.Desktop.Models;

namespace CloudStream.Desktop.Services
{
    /// <summary>
    ///     Provides an abstraction over the SQLite database used by the
    ///     application.  This service is responsible for creating the database
    ///     schema and exposes simple methods for storing and retrieving
    ///     bookmarks, history and resume positions.
    /// </summary>
    public sealed class DatabaseService
    {
        private static readonly Lazy<DatabaseService> _instance = new(() => new DatabaseService());
        private readonly object _syncRoot = new();
        private SqliteConnection? _connection;

        /// <summary>
        ///     Gets the singleton instance of the database service.
        /// </summary>
        public static DatabaseService Instance => _instance.Value;

        private DatabaseService()
        {
        }

        /// <summary>
        ///     Initializes the database.  If the database file does not exist it
        ///     will be created and the necessary tables will be created.
        /// </summary>
        public void Initialize()
        {
            lock (_syncRoot)
            {
                if (_connection != null)
                    return;

                string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string dbPath = Path.Combine(appData, "CloudStream", "cloudstream.db");
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

                bool createSchema = !File.Exists(dbPath);
                _connection = new SqliteConnection($"Data Source={dbPath}");
                _connection.Open();

                if (createSchema)
                {
                    CreateSchema();
                }
            }
        }

        private void CreateSchema()
        {
            using var cmd = _connection!.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Bookmarks (
    Id TEXT PRIMARY KEY,
    ProviderId TEXT NOT NULL,
    MediaId TEXT NOT NULL,
    Title TEXT,
    PosterUrl TEXT,
    Timestamp DATETIME NOT NULL
);
CREATE TABLE IF NOT EXISTS History (
    Id TEXT PRIMARY KEY,
    ProviderId TEXT NOT NULL,
    MediaId TEXT NOT NULL,
    Title TEXT,
    PosterUrl TEXT,
    WatchedAt DATETIME NOT NULL,
    Position INTEGER,
    Duration INTEGER
);
CREATE TABLE IF NOT EXISTS Resume (
    Id TEXT PRIMARY KEY,
    ProviderId TEXT NOT NULL,
    MediaId TEXT NOT NULL,
    Position INTEGER NOT NULL
);";
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        ///     Adds a media item to the bookmarks table.  If the item already
        ///     exists the timestamp is updated to the current time.
        /// </summary>
        public void AddBookmark(MediaItem item)
        {
            EnsureInitialized();
            lock (_syncRoot)
            {
                using var cmd = _connection!.CreateCommand();
                cmd.CommandText = "INSERT OR REPLACE INTO Bookmarks (Id, ProviderId, MediaId, Title, PosterUrl, Timestamp) VALUES (@id, @providerId, @mediaId, @title, @posterUrl, @ts);";
                cmd.Parameters.AddWithValue("@id", $"{item.ProviderId}:{item.Id}");
                cmd.Parameters.AddWithValue("@providerId", item.ProviderId);
                cmd.Parameters.AddWithValue("@mediaId", item.Id);
                cmd.Parameters.AddWithValue("@title", item.Title ?? string.Empty);
                cmd.Parameters.AddWithValue("@posterUrl", (object?)item.PosterUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ts", DateTime.UtcNow);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///     Removes a bookmark identified by provider and media id.
        /// </summary>
        public void RemoveBookmark(string providerId, string mediaId)
        {
            EnsureInitialized();
            lock (_syncRoot)
            {
                using var cmd = _connection!.CreateCommand();
                cmd.CommandText = "DELETE FROM Bookmarks WHERE ProviderId = @providerId AND MediaId = @mediaId";
                cmd.Parameters.AddWithValue("@providerId", providerId);
                cmd.Parameters.AddWithValue("@mediaId", mediaId);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///     Retrieves all bookmarks in order of most recently added.
        /// </summary>
        public IList<MediaItem> GetBookmarks()
        {
            EnsureInitialized();
            var list = new List<MediaItem>();
            lock (_syncRoot)
            {
                using var cmd = _connection!.CreateCommand();
                cmd.CommandText = "SELECT ProviderId, MediaId, Title, PosterUrl FROM Bookmarks ORDER BY Timestamp DESC";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new MediaItem
                    {
                        ProviderId = reader.GetString(0),
                        Id = reader.GetString(1),
                        Title = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        PosterUrl = reader.IsDBNull(3) ? null : reader.GetString(3)
                    });
                }
            }
            return list;
        }

        /// <summary>
        ///     Adds an entry to the history.  If an entry for the same media
        ///     item exists its timestamp and position are updated.
        /// </summary>
        public void AddHistory(MediaItem item, long position, long duration)
        {
            EnsureInitialized();
            lock (_syncRoot)
            {
                using var cmd = _connection!.CreateCommand();
                cmd.CommandText = "INSERT OR REPLACE INTO History (Id, ProviderId, MediaId, Title, PosterUrl, WatchedAt, Position, Duration) VALUES (@id, @providerId, @mediaId, @title, @posterUrl, @watchedAt, @position, @duration);";
                cmd.Parameters.AddWithValue("@id", $"{item.ProviderId}:{item.Id}");
                cmd.Parameters.AddWithValue("@providerId", item.ProviderId);
                cmd.Parameters.AddWithValue("@mediaId", item.Id);
                cmd.Parameters.AddWithValue("@title", item.Title ?? string.Empty);
                cmd.Parameters.AddWithValue("@posterUrl", (object?)item.PosterUrl ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@watchedAt", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@position", position);
                cmd.Parameters.AddWithValue("@duration", duration);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///     Retrieves the most recent history entries.
        /// </summary>
        public IList<(MediaItem Item, long Position, long Duration)> GetHistory(int limit = 50)
        {
            EnsureInitialized();
            var list = new List<(MediaItem, long, long)>();
            lock (_syncRoot)
            {
                using var cmd = _connection!.CreateCommand();
                cmd.CommandText = $"SELECT ProviderId, MediaId, Title, PosterUrl, Position, Duration FROM History ORDER BY WatchedAt DESC LIMIT {limit}";
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var item = new MediaItem
                    {
                        ProviderId = reader.GetString(0),
                        Id = reader.GetString(1),
                        Title = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                        PosterUrl = reader.IsDBNull(3) ? null : reader.GetString(3)
                    };
                    long pos = reader.IsDBNull(4) ? 0 : reader.GetInt64(4);
                    long dur = reader.IsDBNull(5) ? 0 : reader.GetInt64(5);
                    list.Add((item, pos, dur));
                }
            }
            return list;
        }

        /// <summary>
        ///     Saves the resume position for a media item.  This table stores only
        ///     the most recent position and will be updated whenever the user
        ///     stops playback.
        /// </summary>
        public void SaveResumePosition(string providerId, string mediaId, long position)
        {
            EnsureInitialized();
            lock (_syncRoot)
            {
                using var cmd = _connection!.CreateCommand();
                cmd.CommandText = "INSERT OR REPLACE INTO Resume (Id, ProviderId, MediaId, Position) VALUES (@id, @providerId, @mediaId, @position);";
                cmd.Parameters.AddWithValue("@id", $"{providerId}:{mediaId}");
                cmd.Parameters.AddWithValue("@providerId", providerId);
                cmd.Parameters.AddWithValue("@mediaId", mediaId);
                cmd.Parameters.AddWithValue("@position", position);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        ///     Returns the saved resume position for a media item, or null if
        ///     none exists.
        /// </summary>
        public long? GetResumePosition(string providerId, string mediaId)
        {
            EnsureInitialized();
            lock (_syncRoot)
            {
                using var cmd = _connection!.CreateCommand();
                cmd.CommandText = "SELECT Position FROM Resume WHERE ProviderId = @providerId AND MediaId = @mediaId";
                cmd.Parameters.AddWithValue("@providerId", providerId);
                cmd.Parameters.AddWithValue("@mediaId", mediaId);
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    return Convert.ToInt64(result);
                }
                return null;
            }
        }

        private void EnsureInitialized()
        {
            if (_connection == null)
            {
                throw new InvalidOperationException("DatabaseService has not been initialized. Call Initialize() before using.");
            }
        }

        /// <summary>
        ///     Removes all entries from the history table.  This can be invoked
        ///     from the settings page when the user chooses to clear their
        ///     continue watching list.
        /// </summary>
        public void ClearHistory()
        {
            EnsureInitialized();
            lock (_syncRoot)
            {
                using var cmd = _connection!.CreateCommand();
                cmd.CommandText = "DELETE FROM History";
                cmd.ExecuteNonQuery();
            }
        }
    }
}