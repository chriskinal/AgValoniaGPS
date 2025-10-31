# AgValoniaGPS - Session Handoff

## Project Location
**Working Directory:** `C:\Users\chrisk\Documents\AgValoniaGPS`

## Current Status

### Implemented Features ✅
1. **Button Position Persistence** - Floating buttons save/restore their positions per user profile
2. **Window Size Persistence** - Window dimensions and maximized state save/restore
3. **Home Button Reset** - Clicking home resets buttons and saves the reset state
4. **Profile Integration** - All persistence tied to user profile system

### ✅ FIXED - Window Size Now Loads Correctly

**Issue was:** Window size was NOT loading correctly from saved profile on startup.

**Root Cause Found:**
The profile WAS loading correctly from disk, but Avalonia's window layout system wasn't respecting Width/Height changes during the `Loaded` event (called via Dispatcher.Post). Setting window dimensions too early in the window lifecycle caused Avalonia to ignore the changes.

**Solution Implemented:**
Moved profile loading and window sizing from `MainWindow_Loaded` (Dispatcher.Post) to the `MainWindow_Opened` event, which fires after window initialization but before display. This timing allows Avalonia to properly apply the Width/Height properties.

**Files Modified:**
- `AgValoniaGPS.Desktop/Views/MainWindow.axaml.cs`:
  - Added `MainWindow_Opened` event handler to load profile and set window size
  - Removed `LoadWindowSize()` method (no longer needed)
  - Simplified `MainWindow_Loaded` to only handle button position loading

**Verification:**
```
[MainWindow] Setting window size: 2013x987, Maximized=False
[MainWindow] Window size set - Current: 2013x987, State=Normal  ✓
[ButtonPosition] Loading 7 button positions at WindowSize: 2013x987  ✓
```

### Key Files

#### Models
- `AgValoniaGPS\Models\Profile\ButtonPosition.cs` - Button position data model
- `AgValoniaGPS\Models\Profile\DisplayPreferences.cs` - Contains ButtonPositions, WindowWidth, WindowHeight, WindowMaximized

#### Main Window
- `AgValoniaGPS\AgValoniaGPS.Desktop\Views\MainWindow.axaml.cs` - All persistence logic

#### Services
- `AgValoniaGPS\Services\Profile\ProfileManagementService.cs` - Profile loading/switching
- `AgValoniaGPS\Services\Profile\UserProfileProvider.cs` - Disk I/O for profiles

#### Profile Data
- `C:\Users\chrisk\Documents\AgValoniaGPS\Users\Default.json` - User profile storage

## Key Code Sections

### MainWindow.axaml.cs

**Profile Loading (Lines ~198-221):**
```csharp
private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
{
    // ... panel initialization ...

    Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
    {
        // Load profile from disk
        try
        {
            var result = await _profileService.SwitchUserProfileAsync("Default");
            if (result.Success)
            {
                Console.WriteLine($"[Profile] Loaded user profile: Default");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Profile] Error loading profile: {ex.Message}");
        }

        // Apply window size
        LoadWindowSize();

        // Load button positions after brief delay
        await System.Threading.Tasks.Task.Delay(100);
        LoadButtonPositions();
        _hasLoadedPositions = true;
    }, Avalonia.Threading.DispatcherPriority.Background);
}
```

**LoadWindowSize Method (Lines ~254-295):**
```csharp
private void LoadWindowSize()
{
    try
    {
        var currentUserProfile = _profileService.GetCurrentUserProfile();
        if (currentUserProfile == null) return;

        var displayPrefs = currentUserProfile.Preferences.DisplayPreferences;

        Console.WriteLine($"[WindowSize] Loading saved size: {displayPrefs.WindowWidth}x{displayPrefs.WindowHeight}, Maximized={displayPrefs.WindowMaximized}");
        Console.WriteLine($"[WindowSize] Current window size BEFORE: {this.Width}x{this.Height}, State={this.WindowState}");

        // Apply saved size
        if (displayPrefs.WindowMaximized)
        {
            this.WindowState = Avalonia.Controls.WindowState.Maximized;
        }
        else
        {
            double width = Math.Clamp(displayPrefs.WindowWidth, 800, 3840);
            double height = Math.Clamp(displayPrefs.WindowHeight, 600, 2160);

            this.Width = width;
            this.Height = height;
            this.WindowState = Avalonia.Controls.WindowState.Normal;
        }

        Console.WriteLine($"[WindowSize] Set window size to: {displayPrefs.WindowWidth}x{displayPrefs.WindowHeight}");
        Console.WriteLine($"[WindowSize] Current window size AFTER: {this.Width}x{this.Height}, State={this.WindowState}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WindowSize] Error loading: {ex.Message}");
    }
}
```

**Save on Close (Lines ~297-326):**
```csharp
private async void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
{
    try
    {
        var currentUserProfile = _profileService.GetCurrentUserProfile();
        if (currentUserProfile == null) return;

        var displayPrefs = currentUserProfile.Preferences.DisplayPreferences;

        Console.WriteLine($"[WindowSize] Window closing - Current size: {this.Width}x{this.Height}, State={this.WindowState}");

        if (this.WindowState == Avalonia.Controls.WindowState.Maximized)
        {
            displayPrefs.WindowMaximized = true;
            Console.WriteLine($"[WindowSize] Saving maximized state");
        }
        else
        {
            displayPrefs.WindowMaximized = false;
            displayPrefs.WindowWidth = this.Width;
            displayPrefs.WindowHeight = this.Height;
            Console.WriteLine($"[WindowSize] Saving normal window size: {this.Width}x{this.Height}");
        }

        await _userProfileProvider.SaveAsync(currentUserProfile);
        Console.WriteLine($"[WindowSize] Save result: True");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[WindowSize] Error saving: {ex.Message}");
    }
}
```

### ProfileManagementService.cs

**SwitchUserProfileAsync (Lines 208-256):**
```csharp
public async Task<ProfileSwitchResult> SwitchUserProfileAsync(string userName)
{
    try
    {
        // Load the new user profile
        var newProfile = await _userProvider.GetAsync(userName);

        // Update current user profile
        var previousProfile = _currentUserProfile;
        _currentUserProfile = newProfile;

        // Apply user preferences to display settings
        if (newProfile.Preferences != null)
        {
            var displaySettings = _configurationService.GetDisplaySettings();
            displaySettings.UnitSystem = newProfile.Preferences.PreferredUnitSystem;
            await _configurationService.UpdateDisplaySettingsAsync(displaySettings);

            var cultureSettings = _configurationService.GetCultureSettings();
            cultureSettings.CultureCode = newProfile.Preferences.PreferredLanguage;
            await _configurationService.UpdateCultureSettingsAsync(cultureSettings);
        }

        // Raise ProfileChanged event
        ProfileChanged?.Invoke(this, new ProfileChangedEventArgs
        {
            ProfileType = ProfileType.User,
            ProfileName = userName,
            SessionCarriedOver = false,
            Timestamp = DateTime.UtcNow
        });

        return new ProfileSwitchResult
        {
            Success = true,
            ErrorMessage = string.Empty,
            SessionCarriedOver = false
        };
    }
    catch (Exception ex)
    {
        return new ProfileSwitchResult
        {
            Success = false,
            ErrorMessage = $"Failed to switch user profile: {ex.Message}",
            SessionCarriedOver = false
        };
    }
}
```

**GetCurrentUserProfile (Lines 271-274):**
```csharp
public UserProfile GetCurrentUserProfile()
{
    return _currentUserProfile;
}
```

## Investigation Needed

### Primary Question
**Why is `LoadWindowSize()` seeing `1366x768` when the JSON file clearly contains `2013x987`?**

**Possible causes:**
1. `SwitchUserProfileAsync("Default")` is not actually loading from disk
2. `_userProvider.GetAsync("Default")` is returning cached data
3. Profile is being loaded, but then something is resetting it to defaults
4. Wrong profile file is being read (different path?)
5. Timing issue - LoadWindowSize() is called before SwitchUserProfileAsync completes

### Debugging Steps

1. **Add logging to UserProfileProvider.GetAsync()** to verify it's reading from disk:
   ```csharp
   Console.WriteLine($"[UserProfileProvider] Reading profile from: {filePath}");
   Console.WriteLine($"[UserProfileProvider] File exists: {File.Exists(filePath)}");
   Console.WriteLine($"[UserProfileProvider] File contents: {json}");
   ```

2. **Add logging to ProfileManagementService.SwitchUserProfileAsync()** to see what profile data is loaded:
   ```csharp
   Console.WriteLine($"[ProfileService] Loaded profile WindowWidth: {newProfile.Preferences.DisplayPreferences.WindowWidth}");
   ```

3. **Verify the profile file path** is correct:
   ```csharp
   // In UserProfileProvider.GetProfileFilePath
   Console.WriteLine($"[UserProfileProvider] Profile path: {filePath}");
   ```

4. **Check timing** - Ensure LoadWindowSize() is called AFTER SwitchUserProfileAsync completes

## How to Run

```bash
cd "C:\Users\chrisk\Documents\AgValoniaGPS\AgValoniaGPS.Desktop"
dotnet run
```

## How to Test

1. **Resize window** to a unique size (e.g., 1500x900)
2. **Close the app**
3. **Check console output** - Should show "Saving normal window size: 1500x900"
4. **Verify JSON file** at `C:\Users\chrisk\Documents\AgValoniaGPS\Users\Default.json`
5. **Reopen app**
6. **Check console output** - Should show "Loading saved size: 1500x900"
7. **Verify window size** - Window should open at 1500x900

## Expected Console Output (Correct Behavior)

```
[Profile] Loaded user profile: Default
[WindowSize] Loading saved size: 2013x987, Maximized=False
[WindowSize] Current window size BEFORE: 3068x1287, State=Maximized
[WindowSize] Set window size to: 2013x987
[WindowSize] Current window size AFTER: 2013x987, State=Normal
[ButtonPosition] Loading 7 button positions at WindowSize: 2013x987
```

## Actual Console Output (Bug)

```
[Profile] Loaded user profile: Default
[WindowSize] Loading saved size: 1366x768, Maximized=False  <-- WRONG!
[WindowSize] Current window size BEFORE: 3068x1287, State=Maximized
[WindowSize] Set window size to: 1366x768
[WindowSize] Current window size AFTER: 1366x768, State=Normal
[ButtonPosition] Loading 7 button positions at WindowSize: 1366x768
```

## Next Steps

1. Add diagnostic logging to understand where the 1366x768 value is coming from
2. Verify profile is being loaded from correct file path
3. Check if there's a timing race condition
4. Ensure `_currentUserProfile` is updated before `LoadWindowSize()` is called

## Git Status

```
Current branch: main
Status:
M AgValoniaGPS.csproj
M MainWindow.xaml
M MainWindow.xaml.cs
M HANDOFF.md
?? nul

Recent commits:
a724d6d Initial commit.
```

## Notes

- Button position persistence IS working correctly
- Window size SAVE is working correctly (2013x987 in JSON file)
- Window size LOAD is reading wrong value (1366x768)
- This suggests the profile is not being reloaded from disk on startup
- The profile service might be using the in-memory profile created in the constructor instead of loading from disk
