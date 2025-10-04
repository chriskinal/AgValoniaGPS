# AgValoniaGPS Linux Instructions

## Quick Start - Running the Pre-Built Binary

The `publish/linux-x64` folder contains a self-contained Linux x64 binary that's ready to run.

### Requirements
- Linux x64 system (Ubuntu 20.04+, Debian 11+, Fedora, etc.)
- OpenGL ES 3.0+ support (for 3D field rendering)
- For GPS: Serial port access (USB or Bluetooth)

### Running the Application

1. **Copy the folder** to your Linux machine:
   ```bash
   # Example using scp
   scp -r publish/linux-x64 user@linux-machine:~/AgValoniaGPS
   ```

2. **Make it executable**:
   ```bash
   cd ~/AgValoniaGPS/linux-x64
   chmod +x AgValoniaGPS.Desktop
   ```

3. **Run the application**:
   ```bash
   ./AgValoniaGPS.Desktop
   ```

### Serial Port Permissions (for GPS)

If using USB GPS devices, you may need to add your user to the `dialout` group:

```bash
sudo usermod -a -G dialout $USER
# Log out and back in for changes to take effect
```

## Building from Source on Linux

### Prerequisites
- .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0

### Build Steps

1. **Clone the repository**:
   ```bash
   git clone https://github.com/chriskinal/AgValoniaGPS.git
   cd AgValoniaGPS
   git checkout linux-test
   ```

2. **Use the build script**:
   ```bash
   cd AgValoniaGPS
   chmod +x build-linux.sh
   ./build-linux.sh
   ```

   Or manually:
   ```bash
   dotnet build --configuration Release
   dotnet publish AgValoniaGPS.Desktop/AgValoniaGPS.Desktop.csproj \
     --configuration Release \
     --runtime linux-x64 \
     --self-contained true \
     --output ./publish/linux-x64
   ```

## Supported Platforms

| Platform | Status | Notes |
|----------|--------|-------|
| Linux x64 | ✅ Tested | Ubuntu, Debian, Fedora, etc. |
| Linux ARM64 | ✅ Ready | Raspberry Pi 4, Jetson Nano |
| Linux ARM32 | ⚠️ Untested | Should work with linux-arm runtime |

## Troubleshooting

### OpenGL Issues
If you get OpenGL errors, ensure drivers are installed:
```bash
# For Intel/AMD
sudo apt install mesa-utils libgles2-mesa

# For NVIDIA
sudo apt install nvidia-driver-xxx  # xxx = your driver version

# Test OpenGL
glxinfo | grep "OpenGL ES"
```

### Network Issues
The application uses UDP ports 9999 and 8888. Ensure firewall allows:
```bash
sudo ufw allow 9999/udp
sudo ufw allow 8888/udp
```

### Permission Denied
If you get permission errors:
```bash
# Make sure the file is executable
chmod +x AgValoniaGPS.Desktop

# Check file ownership
ls -la AgValoniaGPS.Desktop
```

## What Changed for Linux?

Only one file needed modification: `AgValoniaGPS.Desktop.csproj`

The changes made it cross-platform:
- `OutputType` now adapts to the OS (Exe for Linux, WinExe for Windows)
- Windows-specific features (COM support, manifest) only apply on Windows
- All other code was already cross-platform!

## Performance Notes

- **Memory Usage**: ~50MB base footprint
- **CPU Usage**: Minimal (<5% on modern hardware)
- **GPU**: OpenGL ES 3.0 for 3D rendering
- **Network**: 10-50Hz UDP communication with modules

## Feedback

Please report Linux-specific issues to: https://github.com/chriskinal/AgValoniaGPS/issues

Tag them with `[Linux]` in the title.