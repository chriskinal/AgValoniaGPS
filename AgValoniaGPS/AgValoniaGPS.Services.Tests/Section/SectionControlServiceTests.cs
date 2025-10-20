using System;
using System.Threading;
using AgValoniaGPS.Models;
using AgValoniaGPS.Models.Communication;
using AgValoniaGPS.Models.Events;
using AgValoniaGPS.Models.Section;
using AgValoniaGPS.Services.Communication;
using AgValoniaGPS.Services.Section;
using AgValoniaGPS.Services.GPS;
using NUnit.Framework;

namespace AgValoniaGPS.Services.Tests.Section;

[TestFixture]
public class SectionControlServiceTests
{
    private SectionControlService _service;
    private StubSectionSpeedService _speedService;
    private StubMachineCommunicationService _machineComm;
    private StubAnalogSwitchStateService _switchService;
    private StubSectionConfigurationService _configService;
    private StubPositionUpdateService _positionService;
    private SectionConfiguration _testConfig;

    [SetUp]
    public void SetUp()
    {
        // Create test configuration with short delays for testing
        _testConfig = new SectionConfiguration(3, new double[] { 2.5, 2.5, 2.5 })
        {
            TurnOnDelay = 0.1, // 100ms for fast testing
            TurnOffDelay = 0.1,
            MinimumSpeed = 0.1
        };

        _speedService = new StubSectionSpeedService(3);
        _machineComm = new StubMachineCommunicationService();
        _switchService = new StubAnalogSwitchStateService();
        _configService = new StubSectionConfigurationService(_testConfig);
        _positionService = new StubPositionUpdateService();

        _service = new SectionControlService(
            _speedService,
            _machineComm,
            _switchService,
            _configService,
            _positionService);
    }

    [Test]
    public void UpdateSectionStates_WorkSwitchInactive_AllSectionsTurnOffImmediately()
    {
        // Arrange
        _switchService.SetSwitchState(AnalogSwitchType.WorkSwitch, SwitchState.Inactive);

        var position = new GeoCoord { Easting = 500000, Northing = 4500000 };
        double heading = 0.0;
        double speed = 5.0;

        // Act
        _service.UpdateSectionStates(position, heading, speed);

        // Assert - all sections should be Off
        var states = _service.GetAllSectionStates();
        Assert.That(states.Length, Is.EqualTo(3));
        foreach (var state in states)
        {
            Assert.That(state, Is.EqualTo(SectionState.Off));
        }
    }

    [Test]
    public void UpdateSectionStates_Reversing_AllSectionsTurnOffImmediately()
    {
        // Arrange
        _positionService.SetReversing(true);

        var position = new GeoCoord { Easting = 500000, Northing = 4500000 };
        double heading = 0.0;
        double speed = 5.0;

        // Act
        _service.UpdateSectionStates(position, heading, speed);

        // Assert - all sections should be Off
        var states = _service.GetAllSectionStates();
        foreach (var state in states)
        {
            Assert.That(state, Is.EqualTo(SectionState.Off));
        }
    }

    [Test]
    public void UpdateSectionStates_SpeedBelowMinimum_SectionsTurnOff()
    {
        // Arrange
        var position = new GeoCoord { Easting = 500000, Northing = 4500000 };
        double heading = 0.0;
        double speed = 0.05; // Below minimum (0.1 m/s)

        // Act - call once to start timer
        _service.UpdateSectionStates(position, heading, speed);

        // Wait for timer to expire
        Thread.Sleep(150);

        // Call again to process expired timer
        _service.UpdateSectionStates(position, heading, speed);

        // Assert - sections should be Off after timer expires
        var states = _service.GetAllSectionStates();
        foreach (var state in states)
        {
            Assert.That(state, Is.EqualTo(SectionState.Off));
        }
    }

    [Test]
    public void SetManualOverride_ManualOn_SectionStaysOn()
    {
        // Arrange
        int sectionId = 0;

        // Act
        _service.SetManualOverride(sectionId, SectionState.ManualOn);

        // Assert
        Assert.That(_service.GetSectionState(sectionId), Is.EqualTo(SectionState.ManualOn));
        Assert.That(_service.IsManualOverride(sectionId), Is.True);
    }

    [Test]
    public void SetManualOverride_ManualOff_SectionStaysOff()
    {
        // Arrange
        int sectionId = 0;

        // Act
        _service.SetManualOverride(sectionId, SectionState.ManualOff);

        // Assert
        Assert.That(_service.GetSectionState(sectionId), Is.EqualTo(SectionState.ManualOff));
        Assert.That(_service.IsManualOverride(sectionId), Is.True);
    }

    [Test]
    public void SetManualOverride_ReleaseToAuto_ManualOverrideCleared()
    {
        // Arrange
        int sectionId = 0;
        _service.SetManualOverride(sectionId, SectionState.ManualOn);

        // Act - release to Auto
        _service.SetManualOverride(sectionId, SectionState.Auto);

        // Assert
        Assert.That(_service.GetSectionState(sectionId), Is.EqualTo(SectionState.Auto));
        Assert.That(_service.IsManualOverride(sectionId), Is.False);
    }

    [Test]
    public void UpdateSectionStates_ManualOverride_IgnoresAutomaticControl()
    {
        // Arrange
        int sectionId = 0;
        _service.SetManualOverride(sectionId, SectionState.ManualOn);

        // Set work switch to inactive (would normally turn off sections)
        _switchService.SetSwitchState(AnalogSwitchType.WorkSwitch, SwitchState.Inactive);

        var position = new GeoCoord { Easting = 500000, Northing = 4500000 };

        // Act
        _service.UpdateSectionStates(position, 0.0, 5.0);

        // Assert - manual override section should stay on
        Assert.That(_service.GetSectionState(sectionId), Is.EqualTo(SectionState.ManualOn));
    }

    [Test]
    public void UpdateSectionStates_SectionSpeedBelowThreshold_SectionTurnsOff()
    {
        // Arrange
        int sectionId = 0;
        // Setup section speed below minimum threshold
        _speedService.SetSectionSpeed(sectionId, 0.05);

        var position = new GeoCoord { Easting = 500000, Northing = 4500000 };
        double heading = 0.0;
        double speed = 5.0; // Vehicle speed OK, but section speed low

        // Act - call once to start timer
        _service.UpdateSectionStates(position, heading, speed);

        // Wait for timer to expire
        Thread.Sleep(150);

        // Call again to process expired timer
        _service.UpdateSectionStates(position, heading, speed);

        // Assert - section should be Off
        Assert.That(_service.GetSectionState(sectionId), Is.EqualTo(SectionState.Off));
    }

    [Test]
    public void GetSectionState_InvalidSectionId_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetSectionState(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.GetSectionState(10));
    }

    [Test]
    public void ResetAllSections_ClearsManualOverridesAndTimers()
    {
        // Arrange
        _service.SetManualOverride(0, SectionState.ManualOn);
        _service.SetManualOverride(1, SectionState.ManualOff);

        // Act
        _service.ResetAllSections();

        // Assert
        Assert.That(_service.GetSectionState(0), Is.EqualTo(SectionState.Auto));
        Assert.That(_service.GetSectionState(1), Is.EqualTo(SectionState.Auto));
        Assert.That(_service.IsManualOverride(0), Is.False);
        Assert.That(_service.IsManualOverride(1), Is.False);
    }

    [Test]
    public void SectionStateChanged_Event_RaisedOnStateTransition()
    {
        // Arrange - Initialize sections to Auto state first
        _switchService.SetSwitchState(AnalogSwitchType.WorkSwitch, SwitchState.Active);
        var position = new GeoCoord { Easting = 500000, Northing = 4500000 };
        _service.UpdateSectionStates(position, 0.0, 5.0); // Start sections

        bool eventRaised = false;
        int eventSectionId = -1;
        SectionState eventOldState = SectionState.Off;
        SectionState eventNewState = SectionState.Off;

        _service.SectionStateChanged += (sender, e) =>
        {
            eventRaised = true;
            eventSectionId = e.SectionId;
            eventOldState = e.OldState;
            eventNewState = e.NewState;
        };

        // Act
        _service.SetManualOverride(0, SectionState.ManualOn);

        // Assert
        Assert.That(eventRaised, Is.True);
        Assert.That(eventSectionId, Is.EqualTo(0));
        Assert.That(eventOldState, Is.Not.EqualTo(SectionState.ManualOn), "Old state should be different from ManualOn");
        Assert.That(eventNewState, Is.EqualTo(SectionState.ManualOn));
    }

    #region Stub Implementations

    private class StubSectionSpeedService : ISectionSpeedService
    {
        private double[] _speeds;
        public event EventHandler<SectionSpeedChangedEventArgs>? SectionSpeedChanged;

        public StubSectionSpeedService(int sectionCount)
        {
            _speeds = new double[sectionCount];
            for (int i = 0; i < sectionCount; i++)
            {
                _speeds[i] = 5.0; // Default speed
            }
        }

        public void SetSectionSpeed(int sectionId, double speed)
        {
            _speeds[sectionId] = speed;
        }

        public void CalculateSectionSpeeds(double vehicleSpeed, double turningRadius, double heading)
        {
            // Stub - speeds are set manually in tests
        }

        public double GetSectionSpeed(int sectionId) => _speeds[sectionId];

        public double[] GetAllSectionSpeeds()
        {
            var result = new double[_speeds.Length];
            Array.Copy(_speeds, result, _speeds.Length);
            return result;
        }
    }

    private class StubMachineCommunicationService : IMachineCommunicationService
    {
        public MachineFeedback? CurrentFeedback => null;
        public bool WorkSwitchActive => true;
        public byte[] SectionSensors => Array.Empty<byte>();

        public event EventHandler<MachineFeedbackEventArgs>? FeedbackReceived;
        public event EventHandler<WorkSwitchChangedEventArgs>? WorkSwitchChanged;

        public void SendSectionStates(byte[] sectionStates)
        {
            // Stub - do nothing
        }

        public void SendRelayStates(byte[] relayLo, byte[] relayHi)
        {
            // Stub - do nothing
        }

        public void SendConfiguration(ushort sections, ushort zones, ushort totalWidth)
        {
            // Stub - do nothing
        }
    }

    private class StubAnalogSwitchStateService : IAnalogSwitchStateService
    {
        private SwitchState _workSwitch = SwitchState.Active;
        private SwitchState _steerSwitch = SwitchState.Inactive;
        private SwitchState _lockSwitch = SwitchState.Inactive;

        public event EventHandler<SwitchStateChangedEventArgs>? SwitchStateChanged;

        public SwitchState GetSwitchState(AnalogSwitchType switchType)
        {
            return switchType switch
            {
                AnalogSwitchType.WorkSwitch => _workSwitch,
                AnalogSwitchType.SteerSwitch => _steerSwitch,
                AnalogSwitchType.LockSwitch => _lockSwitch,
                _ => SwitchState.Inactive
            };
        }

        public void SetSwitchState(AnalogSwitchType switchType, SwitchState state)
        {
            var oldState = GetSwitchState(switchType);

            switch (switchType)
            {
                case AnalogSwitchType.WorkSwitch:
                    _workSwitch = state;
                    break;
                case AnalogSwitchType.SteerSwitch:
                    _steerSwitch = state;
                    break;
                case AnalogSwitchType.LockSwitch:
                    _lockSwitch = state;
                    break;
            }

            SwitchStateChanged?.Invoke(this, new SwitchStateChangedEventArgs(switchType, oldState, state));
        }

        public void ResetAllSwitches()
        {
            _workSwitch = SwitchState.Inactive;
            _steerSwitch = SwitchState.Inactive;
            _lockSwitch = SwitchState.Inactive;
        }
    }

    private class StubSectionConfigurationService : ISectionConfigurationService
    {
        private SectionConfiguration _config;

        public event EventHandler? ConfigurationChanged;

        public StubSectionConfigurationService(SectionConfiguration config)
        {
            _config = config;
        }

        public void LoadConfiguration(SectionConfiguration configuration)
        {
            _config = configuration;
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }

        public SectionConfiguration GetConfiguration() => _config;

        public double GetSectionWidth(int sectionId) => _config.SectionWidths[sectionId];

        public double GetTotalWidth() => _config.TotalWidth;

        public double GetSectionOffset(int sectionId)
        {
            double totalWidth = _config.TotalWidth;
            double currentOffset = -totalWidth / 2.0;
            for (int i = 0; i < sectionId; i++)
            {
                currentOffset += _config.SectionWidths[i];
            }
            return currentOffset + (_config.SectionWidths[sectionId] / 2.0);
        }
    }

    private class StubPositionUpdateService : IPositionUpdateService
    {
        private bool _isReversing = false;

        public event EventHandler<PositionUpdateEventArgs>? PositionUpdated;

        public void SetReversing(bool reversing) => _isReversing = reversing;

        public void ProcessGpsPosition(GpsData gpsData, AgValoniaGPS.Models.ImuData? imuData) { }
        public GeoCoord GetCurrentPosition() => new GeoCoord();
        public double GetCurrentHeading() => 0.0;
        public double GetCurrentSpeed() => 0.0;
        public bool IsReversing() => _isReversing;
        public GeoCoord[] GetPositionHistory(int count) => Array.Empty<GeoCoord>();
        public double GetGpsFrequency() => 10.0;
        public void Reset() { }
    }

    #endregion
}
