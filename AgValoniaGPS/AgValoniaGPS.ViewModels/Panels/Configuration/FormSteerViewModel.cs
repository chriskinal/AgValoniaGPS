using AgValoniaGPS.Models;
using AgValoniaGPS.Services.Guidance;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace AgValoniaGPS.ViewModels.Panels.Configuration;

/// <summary>
/// ViewModel for steering configuration panel.
/// Provides UI for configuring steering algorithm settings and vehicle steering parameters.
/// </summary>
public class FormSteerViewModel : PanelViewModelBase
{
    private readonly ISteeringCoordinatorService _steeringService;
    private readonly VehicleConfiguration _vehicleConfig;

    private string _steeringMode = "Stanley";
    private double _minimalLookAhead = 10.0;
    private double _aggressivenessMultiplier = 1.0;
    private double _integralGain = 0.0;
    private double _proportionalGain = 50.0;
    private double _stanleyHeadingErrorGain = 1.0;
    private double _maxSteerAngle = 35.0;
    private double _wheelBase = 2.5;

    public FormSteerViewModel(
        ISteeringCoordinatorService steeringService,
        VehicleConfiguration vehicleConfig)
    {
        _steeringService = steeringService ?? throw new ArgumentNullException(nameof(steeringService));
        _vehicleConfig = vehicleConfig ?? throw new ArgumentNullException(nameof(vehicleConfig));

        Title = "Steering Configuration";

        // Commands
        ApplySettingsCommand = ReactiveCommand.Create(OnApplySettings);
        ResetToDefaultsCommand = ReactiveCommand.Create(OnResetToDefaults);
        TestSteeringCommand = ReactiveCommand.Create(OnTestSteering);

        // Load current settings
        LoadCurrentSettings();
    }

    public string Title { get; } = "Steering Configuration";

    /// <summary>
    /// Steering algorithm mode (Pure Pursuit / Stanley / Hybrid)
    /// </summary>
    public string SteeringMode
    {
        get => _steeringMode;
        set => this.RaiseAndSetIfChanged(ref _steeringMode, value);
    }

    /// <summary>
    /// Minimal look-ahead distance in meters (5-25)
    /// </summary>
    public double MinimalLookAhead
    {
        get => _minimalLookAhead;
        set => this.RaiseAndSetIfChanged(ref _minimalLookAhead, Math.Clamp(value, 5.0, 25.0));
    }

    /// <summary>
    /// Aggressiveness multiplier (0.5-2.0)
    /// </summary>
    public double AggressivenessMultiplier
    {
        get => _aggressivenessMultiplier;
        set => this.RaiseAndSetIfChanged(ref _aggressivenessMultiplier, Math.Clamp(value, 0.5, 2.0));
    }

    /// <summary>
    /// Integral gain (0-1.0)
    /// </summary>
    public double IntegralGain
    {
        get => _integralGain;
        set => this.RaiseAndSetIfChanged(ref _integralGain, Math.Clamp(value, 0.0, 1.0));
    }

    /// <summary>
    /// Proportional gain (0-100)
    /// </summary>
    public double ProportionalGain
    {
        get => _proportionalGain;
        set => this.RaiseAndSetIfChanged(ref _proportionalGain, Math.Clamp(value, 0.0, 100.0));
    }

    /// <summary>
    /// Stanley heading error gain (0-1.0)
    /// </summary>
    public double StanleyHeadingErrorGain
    {
        get => _stanleyHeadingErrorGain;
        set => this.RaiseAndSetIfChanged(ref _stanleyHeadingErrorGain, Math.Clamp(value, 0.0, 1.0));
    }

    /// <summary>
    /// Maximum steering angle in degrees (read from vehicle config)
    /// </summary>
    public double MaxSteerAngle
    {
        get => _maxSteerAngle;
        set => this.RaiseAndSetIfChanged(ref _maxSteerAngle, value);
    }

    /// <summary>
    /// Wheelbase in meters (read from vehicle config)
    /// </summary>
    public double WheelBase
    {
        get => _wheelBase;
        set => this.RaiseAndSetIfChanged(ref _wheelBase, value);
    }

    public ICommand ApplySettingsCommand { get; }
    public ICommand ResetToDefaultsCommand { get; }
    public ICommand TestSteeringCommand { get; }

    private void OnApplySettings()
    {
        try
        {
            // Apply steering algorithm selection
            _steeringService.ActiveAlgorithm = SteeringMode switch
            {
                "Pure Pursuit" => SteeringAlgorithm.PurePursuit,
                "Stanley" => SteeringAlgorithm.Stanley,
                _ => SteeringAlgorithm.Stanley
            };

            // Apply vehicle configuration settings
            _vehicleConfig.MinLookAheadDistance = MinimalLookAhead;
            _vehicleConfig.StanleyHeadingErrorGain = StanleyHeadingErrorGain;
            _vehicleConfig.StanleyDistanceErrorGain = ProportionalGain / 100.0;
            _vehicleConfig.StanleyIntegralGainAB = IntegralGain;
            _vehicleConfig.PurePursuitIntegralGain = IntegralGain;
            _vehicleConfig.MaxSteerAngle = MaxSteerAngle;
            _vehicleConfig.Wheelbase = WheelBase;

            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Failed to apply settings: {ex.Message}");
        }
    }

    private void OnResetToDefaults()
    {
        SteeringMode = "Stanley";
        MinimalLookAhead = 10.0;
        AggressivenessMultiplier = 1.0;
        IntegralGain = 0.0;
        ProportionalGain = 50.0;
        StanleyHeadingErrorGain = 1.0;
        MaxSteerAngle = 35.0;
        WheelBase = 2.5;

        ClearError();
    }

    private void OnTestSteering()
    {
        // Placeholder for steering test command
        // This would send a test command sequence to the AutoSteer module
        // 0 -> max -> 0 steering angle sequence
        try
        {
            // TODO: Implement steering test sequence via AutoSteerCommunicationService
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"Steering test failed: {ex.Message}");
        }
    }

    private void LoadCurrentSettings()
    {
        // Load from vehicle configuration
        SteeringMode = _steeringService.ActiveAlgorithm == SteeringAlgorithm.PurePursuit
            ? "Pure Pursuit"
            : "Stanley";
        MinimalLookAhead = _vehicleConfig.MinLookAheadDistance;
        StanleyHeadingErrorGain = _vehicleConfig.StanleyHeadingErrorGain;
        ProportionalGain = _vehicleConfig.StanleyDistanceErrorGain * 100.0;
        IntegralGain = _vehicleConfig.StanleyIntegralGainAB;
        MaxSteerAngle = _vehicleConfig.MaxSteerAngle;
        WheelBase = _vehicleConfig.Wheelbase;
    }
}
