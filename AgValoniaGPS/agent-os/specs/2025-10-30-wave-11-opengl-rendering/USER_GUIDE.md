# Wave 11 OpenGL Rendering - User Guide

**Date**: 2025-10-30
**Version**: 1.0
**Target Audience**: End users of AgValoniaGPS

---

## Table of Contents

1. [Overview](#overview)
2. [Camera Controls](#camera-controls)
3. [Display Options](#display-options)
4. [Troubleshooting](#troubleshooting)

---

## Overview

The AgValoniaGPS OpenGL Rendering Engine provides a high-performance, interactive map view of your field operations. The map displays:

- **Vehicle Position**: Real-time tractor/implement location and heading
- **Field Boundaries**: Outer field boundary and inner exclusion zones
- **Guidance Lines**: AB lines, curves, and contour paths
- **Coverage Map**: Color-coded visualization of where you've driven
- **Section States**: Real-time display of which sections are on/off
- **Reference Grid**: Coordinate grid with 10-meter spacing

---

## Camera Controls

### Mouse Controls

#### Pan (Move Camera)

**Left-Click and Drag**
- Click and hold the left mouse button
- Drag to move the camera around the field
- Release to stop panning

**Tip**: Pan by dragging the map in the opposite direction you want to move (like Google Maps)

#### Zoom (Change View Scale)

**Mouse Wheel**
- Scroll **up** (away from you) to **zoom in** (closer view)
- Scroll **down** (toward you) to **zoom out** (wider view)
- Zoom is centered on your mouse cursor position

**Zoom Levels**:
- **Close**: 0.1 - 0.5 meters per pixel (detailed view)
- **Medium**: 0.5 - 5 meters per pixel (normal operation)
- **Far**: 5 - 100 meters per pixel (overview)

#### Rotate Camera

**Right-Click and Drag**
- Click and hold the right mouse button
- Drag horizontally to rotate the camera
- Release to stop rotating

**Use Case**: Align the map with your driving direction for easier navigation

### Touch Controls (Tablets/Touch Screens)

#### Single-Finger Pan

- **Touch and drag** with one finger to pan the camera
- Works the same as mouse left-click drag

#### Pinch-to-Zoom

- **Place two fingers** on the screen
- **Pinch together** to zoom out (wider view)
- **Spread apart** to zoom in (closer view)
- Zoom is centered between your two fingers

### Keyboard Controls

#### Arrow Keys - Pan Camera

- **Left Arrow**: Move camera west (left)
- **Right Arrow**: Move camera east (right)
- **Up Arrow**: Move camera north (up)
- **Down Arrow**: Move camera south (down)

**Tip**: Use arrow keys for precise camera positioning

#### Plus/Minus Keys - Zoom

- **+ (Plus)** or **= (Equals)**: Zoom in
- **- (Minus)**: Zoom out

#### Special Keys

- **Home**: Center camera on vehicle position
- **R**: Reset camera rotation to north-up (0 degrees)
- **F**: Fit field to view (or toggle 3D mode if no field loaded)

---

## Display Options

### Grid Display

**What It Shows**:
- Light gray lines every 10 meters
- Brighter lines every 50 meters
- Red line = East-West axis (X-axis)
- Green line = North-South axis (Y-axis)

**Use Case**: Helps estimate distances and provides spatial reference

### Vehicle Display

**What It Shows**:
- Blue tractor icon at current GPS position
- Icon rotates to match vehicle heading
- Size: Approximately 5 meters (scaled with camera zoom)

**Tip**: Press **Home** key to quickly center on the vehicle

### Field Boundary Display

**What It Shows**:
- **Yellow lines**: Outer field boundary
- **Red lines**: Inner boundaries (exclusion zones, obstacles)
- Line width: 5 pixels (automatically scaled)

**Visibility**: Boundaries appear when a field is loaded

### Coverage Map Display

**What It Shows**:
- **Green**: First pass (no overlap)
- **Yellow**: Second pass (single overlap)
- **Orange**: Third+ pass (multiple overlaps)

**Performance**: Automatically reduces detail at far zoom levels for smooth rendering

### 3D Mode (Experimental)

**How to Enable**: Press **F** key or use the 3D toggle button

**What Changes**:
- Camera switches to perspective projection
- Camera follows vehicle from behind and above
- Left-click drag adjusts camera pitch (tilt)
- Mouse wheel adjusts camera distance

**Use Case**: Provides a "chase cam" view for immersive field navigation

**To Return to 2D**: Press **F** key again

---

## Troubleshooting

### Common Issues

#### Map Doesn't Respond to Mouse/Touch

**Possible Causes**:
- Map control not focused
- Another UI element is capturing input

**Solutions**:
1. **Click on the map** to ensure it has focus
2. Check if any popup menus are open and close them
3. Restart the application if issue persists

#### Nothing Visible / Black Screen

**Possible Causes**:
- Camera far from field/vehicle
- No field or vehicle data loaded
- OpenGL initialization failed

**Solutions**:
1. Press **Home** key to center on vehicle
2. Verify GPS connection and field is loaded
3. Check console/log for OpenGL errors
4. Update graphics drivers

#### Poor Performance / Stuttering

**Possible Causes**:
- Too many coverage triangles
- Old/low-end graphics card
- High zoom level with large field

**Solutions**:
1. **Zoom out** to enable Level of Detail (LOD) optimization
2. **Close other applications** to free GPU resources
3. **Update graphics drivers** to latest version
4. **Disable anti-aliasing** if available in settings

#### Incorrect Colors / Visual Artifacts

**Possible Causes**:
- Graphics driver issue
- OpenGL version incompatibility

**Solutions**:
1. **Update graphics drivers**
2. **Restart application**
3. Check minimum requirements: OpenGL 3.3 or higher

#### Zoom Too Sensitive

**Workaround**:
- Use keyboard **+/-** keys for finer zoom control
- Zoom in small increments with mouse wheel

#### Camera Rotation Not Working

**Check**:
- Ensure you're using **right-click** (not left-click)
- Drag **horizontally** (not vertically)
- If still not working, press **R** to reset rotation

---

## Tips & Tricks

### 1. Quick Vehicle Location

Press **Home** key at any time to instantly center the camera on your vehicle. Great for when you get lost!

### 2. Measure Distances

Use the grid lines to estimate distances:
- Small lines = 10 meters apart
- Bright lines = 50 meters apart

### 3. Zoom Techniques

**For Field Overview**:
- Zoom out until entire field is visible
- Use this view for planning passes

**For Precise Navigation**:
- Zoom in close (< 0.5 m/px)
- Watch guidance line alignment in detail

### 4. North-Up Orientation

If the map gets rotated and you lose orientation:
- Press **R** to reset to north-up
- The green axis line points north

### 5. Touch Gestures

On touch devices:
- **Single tap**: Select (future feature)
- **Double tap**: Zoom in (future feature)
- **Two-finger rotate**: Rotate camera (future feature)

### 6. Keyboard Shortcuts Reference

| Key | Action |
|-----|--------|
| **Arrow Keys** | Pan camera (N/S/E/W) |
| **+ / -** | Zoom in / out |
| **Home** | Center on vehicle |
| **R** | Reset rotation to north |
| **F** | Fit field / Toggle 3D |

---

## System Requirements

### Minimum Requirements

- **Operating System**: Windows 10, Linux (Ubuntu 20.04+), macOS 10.15+
- **Graphics**: OpenGL 3.3 or higher
- **RAM**: 4 GB
- **Display**: 1024x768 resolution

### Recommended Requirements

- **Operating System**: Windows 11, Linux (Ubuntu 22.04+), macOS 12+
- **Graphics**: OpenGL 4.5 or higher with dedicated GPU
- **RAM**: 8 GB or more
- **Display**: 1920x1080 resolution (Full HD)

### Graphics Drivers

**Always keep your graphics drivers up to date!**

- **NVIDIA**: Download from [nvidia.com/drivers](https://www.nvidia.com/drivers)
- **AMD**: Download from [amd.com/support](https://www.amd.com/support)
- **Intel**: Download from [intel.com/graphics](https://www.intel.com/content/www/us/en/support/products/80939/graphics.html)

---

## Performance Tips

### For Smooth 60 FPS

1. **Zoom out on large fields**: LOD system reduces triangle count automatically
2. **Close background applications**: Free up GPU resources
3. **Use modern hardware**: Dedicated GPU recommended for large fields
4. **Update drivers**: Latest drivers have performance improvements

### When to Expect Reduced Performance

- **Very large fields** (>1000 hectares) with high coverage
- **Very close zoom** (<0.1 m/px) showing 10,000+ triangles
- **Old graphics cards** (older than 5 years)

**In these cases**: Zoom out slightly to improve performance. The system will automatically reduce detail.

---

## Getting Help

### Check Logs

If experiencing issues, check the application console/log for error messages:

- Look for lines starting with "OpenGL Error:"
- Look for "Shader compilation failed:"
- Look for "ERROR during initialization:"

### Report Bugs

When reporting rendering issues, include:

1. **Graphics card model**: (e.g., "NVIDIA GTX 1060")
2. **OpenGL version**: Check console output at startup
3. **Operating system**: (e.g., "Windows 11")
4. **Screenshot**: If visual issue
5. **Steps to reproduce**: What you were doing when issue occurred

### Known Limitations

- **3D mode is experimental**: May have performance issues on low-end hardware
- **Rotation with coverage map**: Coverage doesn't rotate with camera (planned for future update)
- **Background imagery**: Not yet implemented (planned for Wave 12)
- **Multiple vehicles**: Not yet supported (planned for Wave 13)

---

## FAQ

**Q: Why does the coverage map look less detailed when I zoom out?**

A: This is intentional! The system uses Level of Detail (LOD) to maintain smooth performance. When you zoom back in, full detail returns.

**Q: Can I change the colors of boundaries/coverage?**

A: Not yet. Custom colors are planned for a future update.

**Q: Does the map work offline?**

A: Yes! The map does not require internet connection. It renders your field data locally.

**Q: Why does my vehicle icon disappear sometimes?**

A: If GPS signal is lost, the vehicle icon will disappear. It reappears when GPS signal returns.

**Q: Can I print or export the map view?**

A: Not yet. Screenshot export is planned for a future update. For now, use your OS screenshot tool (e.g., Windows Snipping Tool).

---

## What's Next?

### Upcoming Features (Wave 12+)

- **Background satellite imagery**: See aerial photos under your field data
- **3D terrain**: Elevation-based rendering
- **Multi-vehicle view**: See multiple tractors on same field
- **Path playback**: Review historical driving paths
- **Custom color schemes**: Choose your preferred colors
- **Screenshot export**: Save map images directly from app

---

**End of User Guide**

For technical assistance, consult the [Implementation Guide](./IMPLEMENTATION_GUIDE.md) or contact support.
