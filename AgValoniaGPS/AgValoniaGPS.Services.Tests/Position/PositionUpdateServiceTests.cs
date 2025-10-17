using System;
using System.Threading;
using AgOpenGPS.Core.Models;
using AgValoniaGPS.Services.Position;
using Xunit;

namespace AgValoniaGPS.Services.Tests.Position
{
    /// <summary>
    /// Unit tests for PositionUpdateService covering core functionality:
    /// - Position update processing at 10Hz
    /// - Speed calculation accuracy
    /// - Reverse detection
    /// - Position history management
    /// </summary>
    public class PositionUpdateServiceTests
    {
        [Fact]
        public void ProcessGpsPosition_ValidData_UpdatesCurrentPosition()
        {
            // Arrange
            var service = new PositionUpdateService();
            var gpsData = CreateGpsData(100.0, 200.0, 50.0);

            // Act
            service.ProcessGpsPosition(gpsData, null);

            // Assert
            var position = service.GetCurrentPosition();
            Assert.Equal(100.0, position.Easting, 2);
            Assert.Equal(200.0, position.Northing, 2);
            Assert.Equal(50.0, position.Altitude, 2);
        }

        [Fact]
        public void ProcessGpsPosition_MultipleUpdates_CalculatesSpeed()
        {
            // Arrange
            var service = new PositionUpdateService();

            // Start at origin
            var gpsData1 = CreateGpsData(0.0, 0.0, 0.0);
            service.ProcessGpsPosition(gpsData1, null);

            // Wait to simulate 10Hz (100ms)
            Thread.Sleep(100);

            // Move 1 meter north
            var gpsData2 = CreateGpsData(0.0, 1.0, 0.0);
            service.ProcessGpsPosition(gpsData2, null);

            // Act
            double speed = service.GetCurrentSpeed();

            // Assert - Speed should be approximately 10 m/s (1m in 0.1s)
            // Allow tolerance due to timer precision
            Assert.InRange(speed, 5.0, 15.0);
        }

        [Fact]
        public void ProcessGpsPosition_MinimalMovement_IgnoresUpdate()
        {
            // Arrange
            var service = new PositionUpdateService();
            var gpsData1 = CreateGpsData(0.0, 0.0, 0.0);
            service.ProcessGpsPosition(gpsData1, null);

            // Act - Move less than minimum step distance (0.05m)
            var gpsData2 = CreateGpsData(0.01, 0.01, 0.0);
            service.ProcessGpsPosition(gpsData2, null);

            // Assert - Speed should remain 0 as movement was ignored
            double speed = service.GetCurrentSpeed();
            Assert.Equal(0.0, speed);
        }

        [Fact]
        public void ProcessGpsPosition_ReverseDirection_DetectsReverse()
        {
            // Arrange
            var service = new PositionUpdateService();

            // Establish forward direction (moving north)
            service.ProcessGpsPosition(CreateGpsData(0.0, 0.0, 0.0), null);
            Thread.Sleep(50);
            service.ProcessGpsPosition(CreateGpsData(0.0, 2.0, 0.0), null);
            Thread.Sleep(50);
            service.ProcessGpsPosition(CreateGpsData(0.0, 4.0, 0.0), null);
            Thread.Sleep(50);

            // Act - Reverse direction (move south)
            service.ProcessGpsPosition(CreateGpsData(0.0, 2.0, 0.0), null);

            // Assert
            bool isReversing = service.IsReversing();
            Assert.True(isReversing, "Should detect reverse direction");
        }

        [Fact]
        public void GetPositionHistory_ReturnsCorrectCount()
        {
            // Arrange
            var service = new PositionUpdateService();

            // Add 5 positions
            for (int i = 0; i < 5; i++)
            {
                var gpsData = CreateGpsData(i * 1.0, i * 1.0, 0.0);
                service.ProcessGpsPosition(gpsData, null);
                Thread.Sleep(10);
            }

            // Act
            var history = service.GetPositionHistory(3);

            // Assert
            Assert.Equal(3, history.Length);
        }

        [Fact]
        public void GetPositionHistory_MostRecentFirst()
        {
            // Arrange
            var service = new PositionUpdateService();

            // Add positions in sequence
            service.ProcessGpsPosition(CreateGpsData(0.0, 0.0, 0.0), null);
            Thread.Sleep(50);
            service.ProcessGpsPosition(CreateGpsData(1.0, 1.0, 0.0), null);
            Thread.Sleep(50);
            service.ProcessGpsPosition(CreateGpsData(2.0, 2.0, 0.0), null);

            // Act
            var history = service.GetPositionHistory(3);

            // Assert - Most recent should be at index 0
            Assert.Equal(2.0, history[0].Easting, 2);
            Assert.Equal(1.0, history[1].Easting, 2);
            Assert.Equal(0.0, history[2].Easting, 2);
        }

        [Fact]
        public void Reset_ClearsAllState()
        {
            // Arrange
            var service = new PositionUpdateService();
            service.ProcessGpsPosition(CreateGpsData(10.0, 20.0, 5.0), null);
            Thread.Sleep(50);
            service.ProcessGpsPosition(CreateGpsData(11.0, 21.0, 5.0), null);

            // Act
            service.Reset();

            // Assert
            Assert.Equal(0.0, service.GetCurrentHeading());
            Assert.Equal(0.0, service.GetCurrentSpeed());
            Assert.False(service.IsReversing());

            var history = service.GetPositionHistory(10);
            Assert.Empty(history);
        }

        [Fact]
        public void ProcessGpsPosition_ThreadSafe_HandlesContention()
        {
            // Arrange
            var service = new PositionUpdateService();
            int updateCount = 0;
            service.PositionUpdated += (s, e) => Interlocked.Increment(ref updateCount);

            // Act - Process from multiple threads
            var threads = new Thread[5];
            for (int i = 0; i < threads.Length; i++)
            {
                int threadId = i;
                threads[i] = new Thread(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        var gpsData = CreateGpsData(
                            threadId * 10.0 + j,
                            threadId * 10.0 + j,
                            0.0);
                        service.ProcessGpsPosition(gpsData, null);
                        Thread.Sleep(1);
                    }
                });
                threads[i].Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            // Assert - No exceptions thrown, some updates processed
            Assert.True(updateCount > 0, "Should have processed some updates");
        }

        private static GpsData CreateGpsData(double easting, double northing, double altitude)
        {
            return new GpsData
            {
                Easting = easting,
                Northing = northing,
                Altitude = altitude,
                Latitude = 0.0,
                Longitude = 0.0,
                FixQuality = 4, // RTK Fixed
                SatelliteCount = 12
            };
        }
    }
}
