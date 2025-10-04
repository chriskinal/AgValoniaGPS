@echo off
REM AgValoniaGPS Linux Build Script (from Windows)
REM This script builds and publishes AgValoniaGPS for Linux from a Windows machine

echo =======================================
echo AgValoniaGPS Linux Build Script
echo Building for Linux from Windows
echo =======================================
echo.

REM Clean previous builds
echo Cleaning previous builds...
dotnet clean

REM Restore dependencies
echo.
echo Restoring dependencies...
dotnet restore

REM Build the application
echo.
echo Building AgValoniaGPS...
dotnet build --configuration Release
if %ERRORLEVEL% neq 0 (
    echo Build failed!
    pause
    exit /b %ERRORLEVEL%
)
echo Build succeeded!

REM Publish for Linux x64
echo.
echo Publishing self-contained Linux x64 binary...
dotnet publish AgValoniaGPS.Desktop\AgValoniaGPS.Desktop.csproj ^
    --configuration Release ^
    --runtime linux-x64 ^
    --self-contained true ^
    --output .\publish\linux-x64 ^
    /p:PublishSingleFile=false ^
    /p:PublishTrimmed=false

if %ERRORLEVEL% neq 0 (
    echo Publish failed!
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo =======================================
echo Linux x64 publish succeeded!
echo Output location: %CD%\publish\linux-x64\
echo =======================================
echo.
echo To run on Linux:
echo   1. Copy the 'publish\linux-x64' folder to your Linux machine
echo   2. On Linux, run: chmod +x AgValoniaGPS.Desktop
echo   3. Run: ./AgValoniaGPS.Desktop
echo.

REM Optional: Build for ARM64
set /p BUILD_ARM="Do you want to build for ARM64 (Raspberry Pi)? (y/n): "
if /i "%BUILD_ARM%"=="y" (
    echo.
    echo Publishing self-contained Linux ARM64 binary...
    dotnet publish AgValoniaGPS.Desktop\AgValoniaGPS.Desktop.csproj ^
        --configuration Release ^
        --runtime linux-arm64 ^
        --self-contained true ^
        --output .\publish\linux-arm64 ^
        /p:PublishSingleFile=false ^
        /p:PublishTrimmed=false

    if %ERRORLEVEL% eq 0 (
        echo.
        echo ARM64 publish succeeded!
        echo ARM64 output location: %CD%\publish\linux-arm64\
    ) else (
        echo ARM64 publish failed!
    )
)

echo.
echo =======================================
echo Build complete!
echo =======================================
pause