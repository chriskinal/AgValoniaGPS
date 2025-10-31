using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.FieldOperations;
using AgValoniaGPS.Services.FieldOperations;

namespace ElevationValidation;

/// <summary>
/// Validation program for elevation services
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Elevation Service Validation ===\n");

        var service = new ElevationService();
        var fileService = new ElevationFileService();

        // Test 1: Grid resolution
        Console.WriteLine("Test 1: Default grid resolution");
        Console.WriteLine($"  Default resolution: {service.GridResolution}m");
        service.SetGridResolution(10.0);
        Console.WriteLine($"  Updated resolution: {service.GridResolution}m");
        Console.WriteLine("  ✓ PASS\n");

        // Test 2: Add elevation points
        Console.WriteLine("Test 2: Adding elevation points");
        service.AddElevationPoint(new Position2D(0, 0), 100.0);
        service.AddElevationPoint(new Position2D(10, 0), 110.0);
        service.AddElevationPoint(new Position2D(0, 10), 120.0);
        service.AddElevationPoint(new Position2D(10, 10), 130.0);
        Console.WriteLine($"  Points added: {service.PointCount}");
        Console.WriteLine($"  Min elevation: {service.MinElevation:F2}m");
        Console.WriteLine($"  Max elevation: {service.MaxElevation:F2}m");
        Console.WriteLine($"  Avg elevation: {service.AverageElevation:F2}m");
        Console.WriteLine("  ✓ PASS\n");

        // Test 3: Exact retrieval
        Console.WriteLine("Test 3: Exact elevation retrieval");
        var elev1 = service.GetElevationAt(new Position2D(0, 0));
        Console.WriteLine($"  Elevation at (0,0): {elev1:F2}m (expected 100.00m)");
        if (Math.Abs(elev1.Value - 100.0) < 0.001)
            Console.WriteLine("  ✓ PASS\n");
        else
            Console.WriteLine("  ✗ FAIL\n");

        // Test 4: Bilinear interpolation
        Console.WriteLine("Test 4: Bilinear interpolation");
        var stopwatch = Stopwatch.StartNew();
        var elevCenter = service.InterpolateBilinear(new Position2D(5, 5));
        stopwatch.Stop();
        Console.WriteLine($"  Elevation at (5,5): {elevCenter:F2}m (expected 115.00m)");
        Console.WriteLine($"  Interpolation time: {stopwatch.Elapsed.TotalMilliseconds:F3}ms");
        if (Math.Abs(elevCenter.Value - 115.0) < 0.001 && stopwatch.Elapsed.TotalMilliseconds < 1.0)
            Console.WriteLine("  ✓ PASS - Interpolation accurate and under 1ms\n");
        else
            Console.WriteLine("  ✗ FAIL\n");

        // Test 5: Performance test
        Console.WriteLine("Test 5: Performance test (1000 interpolations)");
        stopwatch.Restart();
        for (int i = 0; i < 1000; i++)
        {
            var x = 5.0 + (i % 10) * 0.1;
            var y = 5.0 + (i / 10) * 0.1;
            service.InterpolateBilinear(new Position2D(x, y));
        }
        stopwatch.Stop();
        var avgTime = stopwatch.Elapsed.TotalMilliseconds / 1000.0;
        Console.WriteLine($"  Average time per interpolation: {avgTime:F4}ms");
        if (avgTime < 1.0)
            Console.WriteLine("  ✓ PASS - Under 1ms per query\n");
        else
            Console.WriteLine("  ✗ FAIL\n");

        // Test 6: File I/O
        Console.WriteLine("Test 6: File save/load round-trip");
        var testDir = Path.Combine(Path.GetTempPath(), $"ElevationTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(testDir);
        try
        {
            await fileService.SaveElevationDataAsync(testDir, service.ElevationGrid);
            var loadedGrid = await fileService.LoadElevationDataAsync(testDir);
            Console.WriteLine($"  Original points: {service.PointCount}");
            Console.WriteLine($"  Loaded points: {loadedGrid.PointCount}");
            Console.WriteLine($"  Original resolution: {service.GridResolution}m");
            Console.WriteLine($"  Loaded resolution: {loadedGrid.GridResolution}m");

            if (loadedGrid.PointCount == service.PointCount &&
                Math.Abs(loadedGrid.GridResolution - service.GridResolution) < 0.001)
                Console.WriteLine("  ✓ PASS\n");
            else
                Console.WriteLine("  ✗ FAIL\n");
        }
        finally
        {
            Directory.Delete(testDir, true);
        }

        // Test 7: Large grid test
        Console.WriteLine("Test 7: Large grid (100x100 = 10,000 points)");
        var largeService = new ElevationService();
        largeService.SetGridResolution(5.0);

        stopwatch.Restart();
        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            {
                double elevation = 100.0 + x * 0.5 + y * 0.5;
                largeService.AddElevationPoint(new Position2D(x * 5.0, y * 5.0), elevation);
            }
        }
        stopwatch.Stop();
        Console.WriteLine($"  Points added: {largeService.PointCount}");
        Console.WriteLine($"  Time to add: {stopwatch.Elapsed.TotalMilliseconds:F2}ms");

        stopwatch.Restart();
        var elevTest = largeService.InterpolateBilinear(new Position2D(250, 250));
        stopwatch.Stop();
        Console.WriteLine($"  Interpolation time: {stopwatch.Elapsed.TotalMilliseconds:F3}ms");

        if (largeService.PointCount == 10000 && stopwatch.Elapsed.TotalMilliseconds < 1.0)
            Console.WriteLine("  ✓ PASS\n");
        else
            Console.WriteLine("  ✗ FAIL\n");

        Console.WriteLine("=== Validation Complete ===");
    }
}
