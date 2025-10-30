using System;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.UI;

/// <summary>
/// Service for playing audio notifications for various system events.
/// Provides cross-platform audio playback with configurable enable/disable settings.
/// </summary>
/// <remarks>
/// Migrated from legacy CSound.cs to support cross-platform audio in Avalonia.
/// Manages playback of notification sounds for:
/// - Boundary alarms
/// - Auto-steer events
/// - Section control events
/// - RTK GPS status changes
/// - Hydraulic lift changes
/// - U-turn warnings
///
/// All audio playback is async and non-blocking.
/// Thread-safe for concurrent access from multiple threads.
/// </remarks>
public interface IAudioNotificationService
{
    #region Event Subscriptions

    /// <summary>
    /// Event raised when a sound is about to be played (before enable/disable check).
    /// Allows UI to display visual feedback synchronized with audio.
    /// </summary>
    event EventHandler<AudioNotificationEventArgs>? NotificationPlaying;

    #endregion

    #region Sound Playback

    /// <summary>
    /// Plays the specified audio notification if enabled.
    /// Non-blocking async operation.
    /// </summary>
    /// <param name="notificationType">Type of notification to play</param>
    /// <remarks>
    /// Checks if the notification type is enabled before playing.
    /// Returns immediately; sound plays asynchronously.
    /// </remarks>
    void PlayNotification(AudioNotificationType notificationType);

    /// <summary>
    /// Stops all currently playing audio notifications.
    /// </summary>
    void StopAllNotifications();

    #endregion

    #region Enable/Disable Settings

    /// <summary>
    /// Gets or sets whether auto-steer sounds are enabled.
    /// Controls AutoSteerOn and AutoSteerOff notifications.
    /// </summary>
    bool IsAutoSteerSoundEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether hydraulic lift sounds are enabled.
    /// Controls HydraulicLiftUp and HydraulicLiftDown notifications.
    /// </summary>
    bool IsHydraulicLiftSoundEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether U-turn sounds are enabled.
    /// Controls UTurnTooClose notification.
    /// </summary>
    bool IsUTurnSoundEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether section control sounds are enabled.
    /// Controls SectionOn and SectionOff notifications.
    /// </summary>
    bool IsSectionSoundEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether boundary alarm sounds are enabled.
    /// Controls BoundaryAlarm notification.
    /// </summary>
    bool IsBoundaryAlarmEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether RTK alarm sounds are enabled.
    /// Controls RTKAlarm and RTKRecovered notifications.
    /// </summary>
    bool IsRTKAlarmEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether headland sounds are enabled.
    /// Controls Headland notification.
    /// </summary>
    bool IsHeadlandSoundEnabled { get; set; }

    #endregion

    #region State Queries

    /// <summary>
    /// Gets whether a boundary alarm is currently active/playing.
    /// </summary>
    bool IsBoundaryAlarming { get; }

    /// <summary>
    /// Gets whether an RTK alarm is currently active/playing.
    /// </summary>
    bool IsRTKAlarming { get; }

    #endregion

    #region Audio Asset Management

    /// <summary>
    /// Loads audio assets for the specified notification type.
    /// Should be called during initialization or when audio files change.
    /// </summary>
    /// <param name="notificationType">Type of notification to load</param>
    /// <param name="audioFilePath">Path to audio file (WAV recommended for cross-platform)</param>
    /// <returns>True if audio loaded successfully, false otherwise</returns>
    bool LoadAudioAsset(AudioNotificationType notificationType, string audioFilePath);

    /// <summary>
    /// Checks if audio asset is loaded for the specified notification type.
    /// </summary>
    /// <param name="notificationType">Type of notification to check</param>
    /// <returns>True if audio asset is loaded, false otherwise</returns>
    bool IsAudioAssetLoaded(AudioNotificationType notificationType);

    #endregion
}

/// <summary>
/// Event arguments for audio notification events.
/// </summary>
public class AudioNotificationEventArgs : EventArgs
{
    /// <summary>
    /// Gets the type of notification being played.
    /// </summary>
    public AudioNotificationType NotificationType { get; }

    /// <summary>
    /// Gets whether the notification was actually played (true) or skipped because disabled (false).
    /// </summary>
    public bool WasPlayed { get; }

    /// <summary>
    /// Creates a new AudioNotificationEventArgs.
    /// </summary>
    /// <param name="notificationType">Type of notification</param>
    /// <param name="wasPlayed">Whether the notification was played</param>
    public AudioNotificationEventArgs(AudioNotificationType notificationType, bool wasPlayed)
    {
        NotificationType = notificationType;
        WasPlayed = wasPlayed;
    }
}
