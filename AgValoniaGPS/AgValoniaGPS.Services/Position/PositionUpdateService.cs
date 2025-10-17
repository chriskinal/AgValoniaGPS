using System;
using System.Diagnostics;
using AgOpenGPS.Core.Models;
using AgValoniaGPS.Services.Interfaces;

namespace AgValoniaGPS.Services.Position
{
    /// <summary>
    /// Implements GPS position processing with smoothing, speed calculation, and history management.
    /// Supports 10Hz update rate with thread-safe access to current state.
    /// </summary>
    /// <remarks>
    /// Extracted from FormGPS/Position.designer.cs UpdateFixPosition() method.
    /// Provides clean, testable position processing without UI dependencies.
    /// Integrates with IHeadingCalculatorService for heading calculations.
    /// </remarks>
    public class PositionUpdateService : IPositionUpdateService
    {
        private const int TotalFixSteps = 10;
        private const double MinimumStepDistance = 0.05; // meters
        private const double ReverseDetectionThreshold = 1.57; // ~90 degrees in radians
        private const double TwoPi = Math.PI * 2.0;

        private readonly object _lockObject = new object();
        private readonly Stopwatch _updateTimer = new Stopwatch();
        private readonly GeoCoord[] _stepFixHistory = new GeoCoord[TotalFixSteps];
        private readonly IHeadingCalculatorService _headingService;

        private GeoCoord _currentPosition = new GeoCoord();
        private double _currentHeading = 0.0;
        private double _currentSpeed = 0.0;
        private bool _isReversing = false;
        private double _gpsFrequency = 10.0; // Hz
        private double _distanceCurrentStep = 0.0;
        private int _currentStepIndex = 0;
        private GeoCoord _previousPosition = new GeoCoord();
        private double _previousHeading = 0.0;
        private int _historyCount = 0;

        // Complementary filter for GPS frequency
        private double _filteredFrequency = 10.0;

        public event EventHandler<PositionUpdateEventArgs>? PositionUpdated;

        public PositionUpdateService(IHeadingCalculatorService headingService)
        {
            _headingService = headingService ?? throw new ArgumentNullException(nameof(headingService));

            // Initialize history buffer
            for (int i = 0; i < TotalFixSteps; i++)
            {
                _stepFixHistory[i] = new GeoCoord();
            }

            _updateTimer.Start();

            // Subscribe to heading changes from heading service
            _headingService.HeadingChanged += OnHeadingChanged;
        }

        public void ProcessGpsPosition(GpsData gpsData, ImuData? imuData)
        {
            if (gpsData == null)
                throw new ArgumentNullException(nameof(gpsData));

            lock (_lockObject)
            {
                // Calculate GPS update frequency
                UpdateFrequency();

                // Get position from GPS data
                var newPosition = new GeoCoord
                {
                    Easting = gpsData.Easting,
                    Northing = gpsData.Northing,
                    Altitude = gpsData.Altitude
                };

                // Calculate distance from last position
                _distanceCurrentStep = CalculateDistance(_stepFixHistory[0], newPosition);

                // Only process if we've moved minimum distance (reduces noise at standstill)
                if (_distanceCurrentStep < MinimumStepDistance)
                {
                    // Update current position but don't update history or heading
                    _currentPosition = newPosition;
                    return;
                }

                // Find appropriate step in history for heading calculation
                int headingStepIndex = FindHeadingStepIndex(newPosition);

                if (headingStepIndex >= 0 && _historyCount > headingStepIndex)
                {
                    // Calculate new heading from position delta using heading service
                    var headingData = new FixToFixHeadingData
                    {
                        CurrentEasting = newPosition.Easting,
                        CurrentNorthing = newPosition.Northing,
                        PreviousEasting = _stepFixHistory[headingStepIndex].Easting,
                        PreviousNorthing = _stepFixHistory[headingStepIndex].Northing,
                        MinimumDistance = 1.0
                    };

                    double newHeading = _headingService.CalculateFixToFixHeading(headingData);

                    // Detect reverse direction
                    _isReversing = DetectReverse(newHeading, _currentHeading);

                    // If reversing, adjust heading by 180 degrees
                    if (_isReversing)
                    {
                        newHeading += Math.PI;
                        if (newHeading >= TwoPi) newHeading -= TwoPi;
                    }

                    _currentHeading = NormalizeHeading(newHeading);
                    _previousHeading = _currentHeading;
                }

                // Calculate speed from distance and time
                double timeDelta = 1.0 / _gpsFrequency; // seconds
                _currentSpeed = timeDelta > 0 ? _distanceCurrentStep / timeDelta : 0.0;

                // Update history buffer (circular buffer, newest at index 0)
                ShiftHistoryBuffer();
                _stepFixHistory[0] = newPosition;
                _currentPosition = newPosition;

                if (_historyCount < TotalFixSteps)
                    _historyCount++;

                // Raise event with new position data
                RaisePositionUpdated();
            }
        }

        public GeoCoord GetCurrentPosition()
        {
            lock (_lockObject)
            {
                return new GeoCoord
                {
                    Easting = _currentPosition.Easting,
                    Northing = _currentPosition.Northing,
                    Altitude = _currentPosition.Altitude
                };
            }
        }

        public double GetCurrentHeading()
        {
            lock (_lockObject)
            {
                return _currentHeading;
            }
        }

        public double GetCurrentSpeed()
        {
            lock (_lockObject)
            {
                return _currentSpeed;
            }
        }

        public bool IsReversing()
        {
            lock (_lockObject)
            {
                return _isReversing;
            }
        }

        public GeoCoord[] GetPositionHistory(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive");

            lock (_lockObject)
            {
                int actualCount = Math.Min(count, Math.Min(_historyCount, TotalFixSteps));
                var history = new GeoCoord[actualCount];

                for (int i = 0; i < actualCount; i++)
                {
                    history[i] = new GeoCoord
                    {
                        Easting = _stepFixHistory[i].Easting,
                        Northing = _stepFixHistory[i].Northing,
                        Altitude = _stepFixHistory[i].Altitude
                    };
                }

                return history;
            }
        }

        public double GetGpsFrequency()
        {
            lock (_lockObject)
            {
                return _gpsFrequency;
            }
        }

        public void Reset()
        {
            lock (_lockObject)
            {
                _currentHeading = 0.0;
                _currentSpeed = 0.0;
                _isReversing = false;
                _historyCount = 0;
                _distanceCurrentStep = 0.0;
                _currentStepIndex = 0;

                for (int i = 0; i < TotalFixSteps; i++)
                {
                    _stepFixHistory[i] = new GeoCoord();
                }

                _updateTimer.Restart();
            }
        }

        private void OnHeadingChanged(object? sender, HeadingUpdate e)
        {
            // Update current heading when heading service calculates it
            lock (_lockObject)
            {
                _currentHeading = e.Heading;
            }
        }

        private void UpdateFrequency()
        {
            double elapsedSeconds = _updateTimer.Elapsed.TotalSeconds;
            _updateTimer.Restart();

            if (elapsedSeconds > 0)
            {
                double currentHz = 1.0 / elapsedSeconds;

                // Clamp frequency to reasonable range
                currentHz = Math.Max(3.0, Math.Min(70.0, currentHz));

                // Complementary filter (98% previous, 2% current)
                _filteredFrequency = 0.98 * _filteredFrequency + 0.02 * currentHz;
                _gpsFrequency = _filteredFrequency;
            }
        }

        private int FindHeadingStepIndex(GeoCoord currentPosition)
        {
            // Find the oldest position that's far enough away for accurate heading
            const double minHeadingDistance = 1.0; // meters
            double minDistSquared = minHeadingDistance * minHeadingDistance;

            for (int i = 0; i < Math.Min(_historyCount, TotalFixSteps); i++)
            {
                double distSquared = CalculateDistanceSquared(_stepFixHistory[i], currentPosition);

                if (distSquared > minDistSquared)
                {
                    return i;
                }
            }

            return -1; // Not enough distance for reliable heading
        }

        private double CalculateDistance(GeoCoord from, GeoCoord to)
        {
            double dx = to.Easting - from.Easting;
            double dy = to.Northing - from.Northing;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private double CalculateDistanceSquared(GeoCoord from, GeoCoord to)
        {
            double dx = to.Easting - from.Easting;
            double dy = to.Northing - from.Northing;
            return dx * dx + dy * dy;
        }

        private bool DetectReverse(double newHeading, double currentHeading)
        {
            // Calculate the absolute angular difference
            double delta = Math.Abs(Math.PI - Math.Abs(Math.Abs(newHeading - currentHeading) - Math.PI));

            // If change is > ~90 degrees, we're likely reversing
            return delta > ReverseDetectionThreshold;
        }

        private double NormalizeHeading(double heading)
        {
            // Normalize to 0 to 2π range
            while (heading < 0) heading += TwoPi;
            while (heading >= TwoPi) heading -= TwoPi;
            return heading;
        }

        private void ShiftHistoryBuffer()
        {
            // Shift all positions down by one (circular buffer)
            for (int i = TotalFixSteps - 1; i > 0; i--)
            {
                _stepFixHistory[i] = _stepFixHistory[i - 1];
            }
        }

        private void RaisePositionUpdated()
        {
            PositionUpdated?.Invoke(this, new PositionUpdateEventArgs
            {
                Position = GetCurrentPosition(),
                Heading = _currentHeading,
                Speed = _currentSpeed,
                DistanceTravelled = _distanceCurrentStep,
                IsReversing = _isReversing,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
