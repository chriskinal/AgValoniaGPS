using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AgValoniaGPS.Models.Session;

namespace AgValoniaGPS.Services.Session;

/// <summary>
/// Implements crash recovery file I/O with atomic writes and stale file detection.
/// Stores crash recovery data in Documents/AgValoniaGPS/Sessions/CrashRecovery.json.
/// </summary>
public class CrashRecoveryService : ICrashRecoveryService
{
    private const string SessionsDirectoryName = "Sessions";
    private const string CrashRecoveryFileName = "CrashRecovery.json";
    private const string TempFileExtension = ".tmp";
    private static readonly TimeSpan StaleFileAge = TimeSpan.FromHours(24);

    private readonly string _crashRecoveryFilePath;
    private readonly string _tempFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the CrashRecoveryService class.
    /// </summary>
    public CrashRecoveryService()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var sessionsDirectory = Path.Combine(documentsPath, "AgValoniaGPS", SessionsDirectoryName);

        Directory.CreateDirectory(sessionsDirectory);

        _crashRecoveryFilePath = Path.Combine(sessionsDirectory, CrashRecoveryFileName);
        _tempFilePath = _crashRecoveryFilePath + TempFileExtension;

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Saves a session state snapshot using atomic write operation.
    /// Writes to temp file first, then renames to ensure data integrity.
    /// </summary>
    public async Task SaveSnapshotAsync(SessionState sessionState)
    {
        if (sessionState == null)
        {
            throw new ArgumentNullException(nameof(sessionState));
        }

        try
        {
            // Update snapshot timestamp
            sessionState.LastSnapshotTime = DateTime.UtcNow;

            // Serialize to JSON
            var json = JsonSerializer.Serialize(sessionState, _jsonOptions);

            // Write to temp file first (atomic operation step 1)
            await File.WriteAllTextAsync(_tempFilePath, json);

            // Rename temp file to actual file (atomic operation step 2)
            // If crash occurs before this, old recovery file remains valid
            File.Move(_tempFilePath, _crashRecoveryFilePath, overwrite: true);
        }
        catch (Exception ex)
        {
            // Clean up temp file if it exists
            if (File.Exists(_tempFilePath))
            {
                try
                {
                    File.Delete(_tempFilePath);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }

            throw new InvalidOperationException("Failed to save crash recovery snapshot", ex);
        }
    }

    /// <summary>
    /// Restores session state from crash recovery file.
    /// Checks file age and rejects files older than 24 hours.
    /// </summary>
    public async Task<SessionRestoreResult> RestoreSnapshotAsync()
    {
        if (!HasCrashRecoveryFile())
        {
            return new SessionRestoreResult
            {
                Success = false,
                ErrorMessage = "No crash recovery file found"
            };
        }

        try
        {
            // Check file age
            var fileInfo = new FileInfo(_crashRecoveryFilePath);
            var fileAge = DateTime.UtcNow - fileInfo.LastWriteTimeUtc;

            if (fileAge > StaleFileAge)
            {
                return new SessionRestoreResult
                {
                    Success = false,
                    ErrorMessage = $"Crash recovery file is too old ({fileAge.TotalHours:F1} hours)",
                    CrashTime = fileInfo.LastWriteTimeUtc
                };
            }

            // Read and deserialize JSON
            var json = await File.ReadAllTextAsync(_crashRecoveryFilePath);
            var sessionState = JsonSerializer.Deserialize<SessionState>(json, _jsonOptions);

            if (sessionState == null)
            {
                return new SessionRestoreResult
                {
                    Success = false,
                    ErrorMessage = "Failed to deserialize crash recovery file"
                };
            }

            return new SessionRestoreResult
            {
                Success = true,
                RestoredSession = sessionState,
                CrashTime = sessionState.LastSnapshotTime
            };
        }
        catch (Exception ex)
        {
            return new SessionRestoreResult
            {
                Success = false,
                ErrorMessage = $"Error reading crash recovery file: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Clears the crash recovery file.
    /// </summary>
    public async Task ClearSnapshotAsync()
    {
        await Task.Run(() =>
        {
            if (File.Exists(_crashRecoveryFilePath))
            {
                try
                {
                    File.Delete(_crashRecoveryFilePath);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Failed to clear crash recovery file", ex);
                }
            }
        });
    }

    /// <summary>
    /// Checks if a crash recovery file exists.
    /// </summary>
    public bool HasCrashRecoveryFile()
    {
        return File.Exists(_crashRecoveryFilePath);
    }
}
