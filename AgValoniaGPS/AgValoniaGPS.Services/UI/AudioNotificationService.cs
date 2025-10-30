using System;
using System.Collections.Generic;
using AgValoniaGPS.Models;

namespace AgValoniaGPS.Services.UI;

/// <summary>
/// Cross-platform audio notification service for playing event-based sounds.
/// </summary>
/// <remarks>
/// This service manages audio playback for system events using a pluggable audio player.
/// The actual audio playback is delegated to an IAudioPlayer implementation which
/// can be platform-specific (Windows Media Player, AvaloniaUI audio, etc.).
///
/// Thread-safe implementation using lock synchronization.
/// All playback operations are non-blocking.
///
/// Migration from legacy CSound.cs (Windows Forms + System.Media.SoundPlayer).
/// </remarks>
public class AudioNotificationService : IAudioNotificationService
{
    private readonly object _lock = new object();
    private readonly Dictionary<AudioNotificationType, IAudioPlayer> _audioPlayers;
    private readonly Dictionary<AudioNotificationType, bool> _enabledNotifications;

    private bool _isBoundaryAlarming;
    private bool _isRTKAlarming;

    /// <summary>
    /// Event raised when a notification is about to be played.
    /// </summary>
    public event EventHandler<AudioNotificationEventArgs>? NotificationPlaying;

    /// <summary>
    /// Creates a new AudioNotificationService with default settings.
    /// </summary>
    public AudioNotificationService()
    {
        _audioPlayers = new Dictionary<AudioNotificationType, IAudioPlayer>();
        _enabledNotifications = new Dictionary<AudioNotificationType, bool>();

        // Initialize all notification types as enabled by default
        foreach (AudioNotificationType type in Enum.GetValues(typeof(AudioNotificationType)))
        {
            _enabledNotifications[type] = true;
        }
    }

    #region Sound Playback

    /// <summary>
    /// Plays the specified audio notification if enabled.
    /// </summary>
    /// <param name="notificationType">Type of notification to play</param>
    public void PlayNotification(AudioNotificationType notificationType)
    {
        bool shouldPlay = false;
        IAudioPlayer? player = null;

        lock (_lock)
        {
            // Check if notification type is enabled
            if (!IsNotificationEnabled(notificationType))
            {
                RaiseNotificationPlaying(notificationType, false);
                return;
            }

            // Get audio player if loaded
            if (_audioPlayers.TryGetValue(notificationType, out player))
            {
                shouldPlay = true;

                // Update alarming state for continuous alarms
                if (notificationType == AudioNotificationType.BoundaryAlarm)
                    _isBoundaryAlarming = true;
                else if (notificationType == AudioNotificationType.RTKAlarm)
                    _isRTKAlarming = true;
            }
        }

        if (shouldPlay && player != null)
        {
            // Play audio outside of lock to avoid blocking
            player.Play();
            RaiseNotificationPlaying(notificationType, true);
        }
        else
        {
            RaiseNotificationPlaying(notificationType, false);
        }
    }

    /// <summary>
    /// Stops all currently playing audio notifications.
    /// </summary>
    public void StopAllNotifications()
    {
        lock (_lock)
        {
            foreach (var player in _audioPlayers.Values)
            {
                player.Stop();
            }

            _isBoundaryAlarming = false;
            _isRTKAlarming = false;
        }
    }

    #endregion

    #region Enable/Disable Settings

    /// <summary>
    /// Gets or sets whether auto-steer sounds are enabled.
    /// </summary>
    public bool IsAutoSteerSoundEnabled
    {
        get => GetNotificationGroupEnabled(AudioNotificationType.AutoSteerOn);
        set
        {
            SetNotificationEnabled(AudioNotificationType.AutoSteerOn, value);
            SetNotificationEnabled(AudioNotificationType.AutoSteerOff, value);
        }
    }

    /// <summary>
    /// Gets or sets whether hydraulic lift sounds are enabled.
    /// </summary>
    public bool IsHydraulicLiftSoundEnabled
    {
        get => GetNotificationGroupEnabled(AudioNotificationType.HydraulicLiftUp);
        set
        {
            SetNotificationEnabled(AudioNotificationType.HydraulicLiftUp, value);
            SetNotificationEnabled(AudioNotificationType.HydraulicLiftDown, value);
        }
    }

    /// <summary>
    /// Gets or sets whether U-turn sounds are enabled.
    /// </summary>
    public bool IsUTurnSoundEnabled
    {
        get => GetNotificationGroupEnabled(AudioNotificationType.UTurnTooClose);
        set => SetNotificationEnabled(AudioNotificationType.UTurnTooClose, value);
    }

    /// <summary>
    /// Gets or sets whether section control sounds are enabled.
    /// </summary>
    public bool IsSectionSoundEnabled
    {
        get => GetNotificationGroupEnabled(AudioNotificationType.SectionOn);
        set
        {
            SetNotificationEnabled(AudioNotificationType.SectionOn, value);
            SetNotificationEnabled(AudioNotificationType.SectionOff, value);
        }
    }

    /// <summary>
    /// Gets or sets whether boundary alarm sounds are enabled.
    /// </summary>
    public bool IsBoundaryAlarmEnabled
    {
        get => GetNotificationGroupEnabled(AudioNotificationType.BoundaryAlarm);
        set => SetNotificationEnabled(AudioNotificationType.BoundaryAlarm, value);
    }

    /// <summary>
    /// Gets or sets whether RTK alarm sounds are enabled.
    /// </summary>
    public bool IsRTKAlarmEnabled
    {
        get => GetNotificationGroupEnabled(AudioNotificationType.RTKAlarm);
        set
        {
            SetNotificationEnabled(AudioNotificationType.RTKAlarm, value);
            SetNotificationEnabled(AudioNotificationType.RTKRecovered, value);
        }
    }

    /// <summary>
    /// Gets or sets whether headland sounds are enabled.
    /// </summary>
    public bool IsHeadlandSoundEnabled
    {
        get => GetNotificationGroupEnabled(AudioNotificationType.Headland);
        set => SetNotificationEnabled(AudioNotificationType.Headland, value);
    }

    #endregion

    #region State Queries

    /// <summary>
    /// Gets whether a boundary alarm is currently active.
    /// </summary>
    public bool IsBoundaryAlarming
    {
        get
        {
            lock (_lock)
            {
                return _isBoundaryAlarming;
            }
        }
    }

    /// <summary>
    /// Gets whether an RTK alarm is currently active.
    /// </summary>
    public bool IsRTKAlarming
    {
        get
        {
            lock (_lock)
            {
                return _isRTKAlarming;
            }
        }
    }

    #endregion

    #region Audio Asset Management

    /// <summary>
    /// Loads audio asset for the specified notification type.
    /// </summary>
    /// <param name="notificationType">Type of notification to load</param>
    /// <param name="audioFilePath">Path to audio file</param>
    /// <returns>True if loaded successfully, false otherwise</returns>
    public bool LoadAudioAsset(AudioNotificationType notificationType, string audioFilePath)
    {
        if (string.IsNullOrWhiteSpace(audioFilePath))
            return false;

        try
        {
            // Create platform-specific audio player
            var player = CreateAudioPlayer(audioFilePath);
            if (player == null)
                return false;

            lock (_lock)
            {
                _audioPlayers[notificationType] = player;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if audio asset is loaded for the specified notification type.
    /// </summary>
    /// <param name="notificationType">Type of notification to check</param>
    /// <returns>True if audio asset is loaded</returns>
    public bool IsAudioAssetLoaded(AudioNotificationType notificationType)
    {
        lock (_lock)
        {
            return _audioPlayers.ContainsKey(notificationType);
        }
    }

    #endregion

    #region Private Helper Methods

    private bool IsNotificationEnabled(AudioNotificationType type)
    {
        return _enabledNotifications.TryGetValue(type, out bool enabled) && enabled;
    }

    private void SetNotificationEnabled(AudioNotificationType type, bool enabled)
    {
        lock (_lock)
        {
            _enabledNotifications[type] = enabled;
        }
    }

    private bool GetNotificationGroupEnabled(AudioNotificationType type)
    {
        lock (_lock)
        {
            return _enabledNotifications.TryGetValue(type, out bool enabled) && enabled;
        }
    }

    private void RaiseNotificationPlaying(AudioNotificationType type, bool wasPlayed)
    {
        NotificationPlaying?.Invoke(this, new AudioNotificationEventArgs(type, wasPlayed));
    }

    /// <summary>
    /// Creates a platform-specific audio player for the given file path.
    /// Override or inject factory for different platform implementations.
    /// </summary>
    /// <param name="audioFilePath">Path to audio file</param>
    /// <returns>Audio player instance or null if creation failed</returns>
    /// <remarks>
    /// TODO: Implement platform-specific audio player creation.
    /// Options:
    /// - Windows: System.Media.SoundPlayer or NAudio
    /// - Linux: ALSA/PulseAudio via P/Invoke or library
    /// - macOS: AVFoundation via P/Invoke or library
    /// - Cross-platform: Consider LibVLC, BASS, or similar
    ///
    /// For now, returns a stub player. Platform-specific implementation
    /// should be provided via dependency injection or factory pattern.
    /// </remarks>
    private IAudioPlayer? CreateAudioPlayer(string audioFilePath)
    {
        // TODO: Replace with actual platform-specific implementation
        // For now, return a stub implementation
        return new StubAudioPlayer(audioFilePath);
    }

    #endregion
}

/// <summary>
/// Interface for platform-specific audio playback.
/// Implementations should provide async, non-blocking playback.
/// </summary>
public interface IAudioPlayer
{
    /// <summary>
    /// Plays the audio asynchronously.
    /// Should be non-blocking and return immediately.
    /// </summary>
    void Play();

    /// <summary>
    /// Stops the audio playback immediately.
    /// </summary>
    void Stop();
}

/// <summary>
/// Stub implementation of IAudioPlayer for testing/development.
/// Replace with platform-specific implementation in production.
/// </summary>
internal class StubAudioPlayer : IAudioPlayer
{
    private readonly string _audioFilePath;

    public StubAudioPlayer(string audioFilePath)
    {
        _audioFilePath = audioFilePath;
    }

    public void Play()
    {
        // TODO: Implement actual audio playback
        // For now, this is a no-op stub
        System.Diagnostics.Debug.WriteLine($"[StubAudioPlayer] Playing: {_audioFilePath}");
    }

    public void Stop()
    {
        // TODO: Implement actual audio stop
        System.Diagnostics.Debug.WriteLine($"[StubAudioPlayer] Stopped: {_audioFilePath}");
    }
}
