using System;
using System.Collections.Generic;
using System.Linq;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Guidance;
using AgValoniaGPS.Services.Guidance;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Guidance
{
    /// <summary>
    /// Unit tests for ABLineService advanced operations:
    /// - NudgeLine with positive/negative offsets
    /// - GenerateParallelLines with exact spacing
    /// - Unit conversion support
    /// - Performance requirements (<50ms for 10 lines)
    /// </summary>
    public class ABLineAdvancedTests
    {
        [Fact]
        public void NudgeLine_PositiveOffset_MovesLinePerpendicular()
        {
            // Arrange
            var service = new ABLineService();
            var originalLine = new ABLine
            {
                Name = "Original",
                PointA = new Position { Easting = 0.0, Northing = 0.0 },
                PointB = new Position { Easting = 100.0, Northing = 0.0 },
                Heading = 90.0 // East
            };

            // Act
            var nudgedLine = service.NudgeLine(originalLine, 5.0);

            // Assert
            Assert.NotNull(nudgedLine);
            Assert.Equal(5.0, nudgedLine.NudgeOffset);
            // Nudge perpendicular to heading (90°) should move north (+Y)
            Assert.Equal(5.0, nudgedLine.PointA.Northing, 2);
            Assert.Equal(5.0, nudgedLine.PointB.Northing, 2);
            // Easting should remain the same
            Assert.Equal(0.0, nudgedLine.PointA.Easting, 2);
            Assert.Equal(100.0, nudgedLine.PointB.Easting, 2);
            // Heading should be preserved
            Assert.Equal(90.0, nudgedLine.Heading, 2);
        }

        [Fact]
        public void NudgeLine_NegativeOffset_MovesLineOppositeDirection()
        {
            // Arrange
            var service = new ABLineService();
            var originalLine = new ABLine
            {
                Name = "Original",
                PointA = new Position { Easting = 0.0, Northing = 0.0 },
                PointB = new Position { Easting = 100.0, Northing = 0.0 },
                Heading = 90.0 // East
            };

            // Act
            var nudgedLine = service.NudgeLine(originalLine, -3.5);

            // Assert
            Assert.NotNull(nudgedLine);
            Assert.Equal(-3.5, nudgedLine.NudgeOffset);
            // Negative nudge should move south (-Y)
            Assert.Equal(-3.5, nudgedLine.PointA.Northing, 2);
            Assert.Equal(-3.5, nudgedLine.PointB.Northing, 2);
            Assert.Equal(90.0, nudgedLine.Heading, 2);
        }

        [Fact]
        public void GenerateParallelLines_CreatesCorrectCount()
        {
            // Arrange
            var service = new ABLineService();
            var referenceLine = new ABLine
            {
                Name = "Reference",
                PointA = new Position { Easting = 0.0, Northing = 0.0 },
                PointB = new Position { Easting = 100.0, Northing = 0.0 },
                Heading = 90.0
            };

            // Act
            var parallels = service.GenerateParallelLines(referenceLine, 10.0, 3, UnitSystem.Metric);

            // Assert
            // Should create 3 left + 3 right = 6 total
            Assert.NotNull(parallels);
            Assert.Equal(6, parallels.Count);
        }

        [Fact]
        public void GenerateParallelLines_MaintainsExactSpacing()
        {
            // Arrange
            var service = new ABLineService();
            var referenceLine = new ABLine
            {
                Name = "Reference",
                PointA = new Position { Easting = 0.0, Northing = 0.0 },
                PointB = new Position { Easting = 100.0, Northing = 0.0 },
                Heading = 90.0
            };
            double spacingMeters = 12.0;

            // Act
            var parallels = service.GenerateParallelLines(referenceLine, spacingMeters, 2, UnitSystem.Metric);

            // Assert - Should have 2 left + 2 right = 4 lines
            Assert.Equal(4, parallels.Count);

            // Verify left-side lines (negative offsets)
            var leftLines = parallels.Where(l => l.NudgeOffset < 0).OrderBy(l => l.NudgeOffset).ToList();
            Assert.Equal(-24.0, leftLines[0].NudgeOffset, 2); // 2 * spacing
            Assert.Equal(-12.0, leftLines[1].NudgeOffset, 2); // 1 * spacing

            // Verify right-side lines (positive offsets)
            var rightLines = parallels.Where(l => l.NudgeOffset > 0).OrderBy(l => l.NudgeOffset).ToList();
            Assert.Equal(12.0, rightLines[0].NudgeOffset, 2);  // 1 * spacing
            Assert.Equal(24.0, rightLines[1].NudgeOffset, 2);  // 2 * spacing

            // Verify spacing accuracy (within ±2cm as per spec)
            foreach (var line in parallels)
            {
                double expectedOffset = Math.Abs(line.NudgeOffset);
                double actualSpacing = expectedOffset / Math.Floor(expectedOffset / spacingMeters);
                Assert.InRange(actualSpacing, spacingMeters - 0.02, spacingMeters + 0.02);
            }
        }

        [Fact]
        public void GenerateParallelLines_ImperialUnits_ConvertsCorrectly()
        {
            // Arrange
            var service = new ABLineService();
            var referenceLine = new ABLine
            {
                Name = "Reference",
                PointA = new Position { Easting = 0.0, Northing = 0.0 },
                PointB = new Position { Easting = 100.0, Northing = 0.0 },
                Heading = 90.0
            };
            double spacingFeet = 30.0; // 30 feet
            double spacingMeters = spacingFeet * 0.3048; // Convert to meters

            // Act
            var parallels = service.GenerateParallelLines(referenceLine, spacingFeet, 1, UnitSystem.Imperial);

            // Assert
            Assert.Equal(2, parallels.Count); // 1 left + 1 right

            // Verify offset is in meters (internal representation)
            var leftLine = parallels.First(l => l.NudgeOffset < 0);
            var rightLine = parallels.First(l => l.NudgeOffset > 0);

            Assert.Equal(-spacingMeters, leftLine.NudgeOffset, 2);
            Assert.Equal(spacingMeters, rightLine.NudgeOffset, 2);
        }

        [Fact]
        public void GenerateParallelLines_Performance_CompletesUnder50ms()
        {
            // Arrange
            var service = new ABLineService();
            var referenceLine = new ABLine
            {
                Name = "Reference",
                PointA = new Position { Easting = 0.0, Northing = 0.0 },
                PointB = new Position { Easting = 100.0, Northing = 0.0 },
                Heading = 90.0
            };

            // Act
            var startTime = DateTime.UtcNow;
            var parallels = service.GenerateParallelLines(referenceLine, 10.0, 10, UnitSystem.Metric);
            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Assert
            Assert.Equal(20, parallels.Count); // 10 left + 10 right
            Assert.True(elapsedMs < 50.0,
                $"Parallel line generation took {elapsedMs}ms, expected <50ms");
        }
    }
}
