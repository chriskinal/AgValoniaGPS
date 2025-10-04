#!/bin/bash

# AgValoniaGPS Linux Build Script
# This script builds and publishes AgValoniaGPS for Linux

echo "======================================="
echo "AgValoniaGPS Linux Build Script"
echo "======================================="

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
NC='\033[0m' # No Color

# Get the directory where the script is located
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Change to the project directory
cd "$DIR"

# Clean previous builds
echo ""
echo "Cleaning previous builds..."
dotnet clean

# Restore dependencies
echo ""
echo "Restoring dependencies..."
dotnet restore

# Build the application
echo ""
echo "Building AgValoniaGPS..."
if dotnet build --configuration Release; then
    echo -e "${GREEN}✓ Build succeeded${NC}"
else
    echo -e "${RED}✗ Build failed${NC}"
    exit 1
fi

# Publish for Linux x64
echo ""
echo "Publishing self-contained Linux x64 binary..."
if dotnet publish AgValoniaGPS.Desktop/AgValoniaGPS.Desktop.csproj \
    --configuration Release \
    --runtime linux-x64 \
    --self-contained true \
    --output ./publish/linux-x64 \
    /p:PublishSingleFile=false \
    /p:PublishTrimmed=false; then

    echo -e "${GREEN}✓ Linux x64 publish succeeded${NC}"
    echo ""
    echo "Output location: $DIR/publish/linux-x64/"

    # Make the executable runnable
    chmod +x ./publish/linux-x64/AgValoniaGPS.Desktop

    echo ""
    echo "To run the application:"
    echo "  cd publish/linux-x64"
    echo "  ./AgValoniaGPS.Desktop"
else
    echo -e "${RED}✗ Publish failed${NC}"
    exit 1
fi

# Optional: Publish for Linux ARM64 (Raspberry Pi)
echo ""
read -p "Do you want to build for ARM64 (Raspberry Pi)? (y/n) " -n 1 -r
echo ""
if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "Publishing self-contained Linux ARM64 binary..."
    if dotnet publish AgValoniaGPS.Desktop/AgValoniaGPS.Desktop.csproj \
        --configuration Release \
        --runtime linux-arm64 \
        --self-contained true \
        --output ./publish/linux-arm64 \
        /p:PublishSingleFile=false \
        /p:PublishTrimmed=false; then

        echo -e "${GREEN}✓ Linux ARM64 publish succeeded${NC}"
        echo ""
        echo "ARM64 output location: $DIR/publish/linux-arm64/"

        # Make the executable runnable
        chmod +x ./publish/linux-arm64/AgValoniaGPS.Desktop
    else
        echo -e "${RED}✗ ARM64 publish failed${NC}"
    fi
fi

echo ""
echo "======================================="
echo "Build complete!"
echo "======================================="