# Business Logic Extraction Patterns Guide

## Introduction

This guide provides concrete patterns and code examples for extracting business logic from WinForms event handlers, timers, and UI methods into clean, testable services. Each pattern shows real "before" code from AgOpenGPS and the "after" clean architecture approach.

## Pattern Catalog

### Pattern 1: Timer Tick Extraction
**Problem**: Business logic embedded in timer event handlers

#### Before (WinForms Timer):
```csharp
// FormGPS.Designer.cs - tmrWatchdog_tick
private void tmrWatchdog_tick(object sender, EventArgs e)
{
    // Business Logic: GPS timeout detection
    if (recvCounter > 20) // Magic number!
    {
        lblGPSQuality.BackColor = Color.Red; // UI update
        isGPSConnected = false; // Business state
        StopAutoSteer(); // Business action
        soundManager.PlaySound("GPS_Lost.wav"); // Side effect
    }

    // Business Logic: Frame rate calculation
    if (frameCounter++ >= 4) // Another magic number
    {
        frameRate = frameCounter * 4; // Business calculation
        lblFPS.Text = frameRate.ToString(); // UI update
        frameCounter = 0;
    }

    // Business Logic: Section timing
    for (int i = 0; i < sections.Length; i++)
    {
        if (sections[i].isOn)
        {
            sections[i].workingTime += 0.125; // Hardcoded interval
        }
    }
}
```

#### After (Clean Service):
```csharp
// Services/Monitoring/ConnectionMonitorService.cs
public class ConnectionMonitorService : IConnectionMonitorService
{
    private readonly TimeSpan _gpsTimeout = TimeSpan.FromSeconds(2);
    private DateTime _lastGpsReceived = DateTime.Now;

    public event EventHandler<ConnectionStatus> ConnectionStatusChanged;

    public void CheckConnections()
    {
        var elapsed = DateTime.Now - _lastGpsReceived;
        if (elapsed > _gpsTimeout && IsConnected)
        {
            IsConnected = false;
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatus
            {
                Type = ConnectionType.GPS,
                IsConnected = false,
                Message = "GPS signal lost"
            });
        }
    }

    public void RecordGpsReceived()
    {
        _lastGpsReceived = DateTime.Now;
        if (!IsConnected)
        {
            IsConnected = true;
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatus
            {
                Type = ConnectionType.GPS,
                IsConnected = true
            });
        }
    }
}

// Services/Monitoring/PerformanceMonitorService.cs
public class PerformanceMonitorService : IPerformanceMonitorService
{
    private int _frameCount;
    private DateTime _lastMeasurement = DateTime.Now;

    public int FrameRate { get; private set; }

    public void RecordFrame()
    {
        _frameCount++;

        var elapsed = DateTime.Now - _lastMeasurement;
        if (elapsed.TotalSeconds >= 1.0)
        {
            FrameRate = (int)(_frameCount / elapsed.TotalSeconds);
            _frameCount = 0;
            _lastMeasurement = DateTime.Now;
        }
    }
}

// Services/Section/SectionTimingService.cs
public class SectionTimingService : ISectionTimingService
{
    private readonly Dictionary<int, TimeSpan> _sectionTimes = new();

    public void UpdateSectionTimes(bool[] sectionStates, TimeSpan elapsed)
    {
        for (int i = 0; i < sectionStates.Length; i++)
        {
            if (sectionStates[i])
            {
                if (!_sectionTimes.ContainsKey(i))
                    _sectionTimes[i] = TimeSpan.Zero;

                _sectionTimes[i] += elapsed;
            }
        }
    }

    public TimeSpan GetSectionTime(int sectionIndex) =>
        _sectionTimes.GetValueOrDefault(sectionIndex, TimeSpan.Zero);
}

// ViewModels/MainViewModel.cs - Orchestration
public class MainViewModel
{
    private readonly DispatcherTimer _monitoringTimer;

    public MainViewModel(
        IConnectionMonitorService connectionMonitor,
        IPerformanceMonitorService performanceMonitor,
        ISectionTimingService sectionTiming)
    {
        _monitoringTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(125)
        };

        _monitoringTimer.Tick += (s, e) =>
        {
            // Only orchestration, no business logic
            connectionMonitor.CheckConnections();
            performanceMonitor.RecordFrame();
            sectionTiming.UpdateSectionTimes(SectionStates, _monitoringTimer.Interval);
        };

        // React to business events
        connectionMonitor.ConnectionStatusChanged += (s, status) =>
        {
            IsGpsConnected = status.IsConnected;
            if (!status.IsConnected)
            {
                _autoSteerService.Disable();
                _soundService.PlayAlert("gps_lost");
            }
        };
    }
}
```

---

### Pattern 2: Complex Calculation Extraction
**Problem**: Mathematical algorithms mixed with UI code

#### Before (Calculation in Event Handler):
```csharp
// FormGPS.cs - Button click with embedded calculation
private void btnABLine_Click(object sender, EventArgs e)
{
    // UI State
    btnABLine.Enabled = false;
    btnABLine.Image = Properties.Resources.ABLineOn;

    // Complex Business Logic
    double dx = currentABLine.ptB.easting - currentABLine.ptA.easting;
    double dy = currentABLine.ptB.northing - currentABLine.ptA.northing;

    if (Math.Abs(dx) < 0.01 && Math.Abs(dy) < 0.01)
    {
        MessageBox.Show("Points too close");
        return;
    }

    currentABLine.heading = Math.Atan2(dx, dy);
    if (currentABLine.heading < 0) currentABLine.heading += 2 * Math.PI;

    // Calculate reference line
    double sin = Math.Sin(currentABLine.heading);
    double cos = Math.Cos(currentABLine.heading);

    currentABLine.refPoint1.easting = currentABLine.ptA.easting - (sin * 4000);
    currentABLine.refPoint1.northing = currentABLine.ptA.northing - (cos * 4000);
    currentABLine.refPoint2.easting = currentABLine.ptA.easting + (sin * 4000);
    currentABLine.refPoint2.northing = currentABLine.ptA.northing + (cos * 4000);

    // More UI updates
    lblABHeading.Text = (currentABLine.heading * 57.2957795).ToString("F1");
    isABLineSet = true;
}
```

#### After (Extracted Calculation Service):
```csharp
// Services/Guidance/ABLineCalculator.cs
public class ABLineCalculator : IABLineCalculator
{
    private const double MinimumLineLength = 0.01;
    private const double ExtensionDistance = 4000;

    public ABLineCreationResult CreateABLine(Position pointA, Position pointB)
    {
        var vector = pointB - pointA;

        if (vector.Length < MinimumLineLength)
        {
            return ABLineCreationResult.Failure("Points are too close together");
        }

        var heading = Math.Atan2(vector.Easting, vector.Northing);
        heading = NormalizeHeading(heading);

        var referenceLine = CalculateReferenceLine(pointA, heading, ExtensionDistance);

        return ABLineCreationResult.Success(new ABLine
        {
            PointA = pointA,
            PointB = pointB,
            Heading = heading,
            ReferenceLine = referenceLine
        });
    }

    private static double NormalizeHeading(double heading)
    {
        while (heading < 0) heading += 2 * Math.PI;
        while (heading >= 2 * Math.PI) heading -= 2 * Math.PI;
        return heading;
    }

    private static ReferenceLine CalculateReferenceLine(
        Position origin,
        double heading,
        double extension)
    {
        var sin = Math.Sin(heading);
        var cos = Math.Cos(heading);

        return new ReferenceLine
        {
            Start = new Position(
                origin.Easting - sin * extension,
                origin.Northing - cos * extension),
            End = new Position(
                origin.Easting + sin * extension,
                origin.Northing + cos * extension)
        };
    }

    public double CalculateCrossTrackError(Position current, ABLine line)
    {
        // Pure mathematical calculation
        var dx = line.PointB.Easting - line.PointA.Easting;
        var dy = line.PointB.Northing - line.PointA.Northing;
        var len = Math.Sqrt(dx * dx + dy * dy);

        if (len < 0.00001) return 0;

        return ((dy * current.Easting - dx * current.Northing) +
                (line.PointB.Easting * line.PointA.Northing) -
                (line.PointB.Northing * line.PointA.Easting)) / len;
    }
}

// ViewModels/GuidanceViewModel.cs
public class GuidanceViewModel : ReactiveObject
{
    private readonly IABLineCalculator _calculator;

    public ICommand CreateABLineCommand { get; }

    public GuidanceViewModel(IABLineCalculator calculator)
    {
        _calculator = calculator;

        CreateABLineCommand = ReactiveCommand.Create(() =>
        {
            var result = _calculator.CreateABLine(PointA, PointB);

            if (result.Success)
            {
                CurrentABLine = result.Line;
                ABHeadingDegrees = result.Line.Heading * 180 / Math.PI;
                IsABLineActive = true;
            }
            else
            {
                _messageService.ShowWarning(result.ErrorMessage);
            }
        });
    }
}
```

---

### Pattern 3: State Machine Extraction
**Problem**: State management scattered across form fields and event handlers

#### Before (State in Form Fields):
```csharp
// FormGPS.cs - State scattered everywhere
public partial class FormGPS : Form
{
    // State variables all over the form
    private bool isJobStarted;
    private bool isAreaOnRight;
    private bool isAutoSteerBtnOn;
    private bool isInFreeDriveMode;
    private bool isFirstHeadingSet;
    private bool isPursuiting;
    private int guidanceMode; // 0=Off, 1=AB, 2=Contour, 3=Curve

    private void StartNewJob()
    {
        // State changes scattered through method
        isJobStarted = true;
        isFirstHeadingSet = false;
        isAreaOnRight = true;
        btnSectionOffAutoOn.Enabled = true;
        btnManualOffOn.Enabled = true;
        btnAutoSteer.Enabled = false;
        guidanceMode = 0;
        // ... 50 more state changes
    }

    private void btnAutoSteer_Click(object sender, EventArgs e)
    {
        if (!isJobStarted) return;
        if (guidanceMode == 0)
        {
            MessageBox.Show("No guidance line");
            return;
        }
        if (Math.Abs(speed) < 0.5)
        {
            MessageBox.Show("Too slow");
            return;
        }

        isAutoSteerBtnOn = !isAutoSteerBtnOn;

        if (isAutoSteerBtnOn)
        {
            btnAutoSteer.Image = Properties.Resources.AutoSteerOn;
            isPursuiting = true;
        }
        else
        {
            btnAutoSteer.Image = Properties.Resources.AutoSteerOff;
            isPursuiting = false;
        }
    }
}
```

#### After (State Machine Service):
```csharp
// Services/State/FieldOperationStateMachine.cs
public enum FieldState
{
    Idle,
    FieldOpen,
    Recording,
    GuidanceActive,
    AutoSteerEngaged
}

public enum GuidanceMode
{
    None,
    ABLine,
    Contour,
    Curve
}

public class FieldOperationStateMachine : IFieldOperationStateMachine
{
    private FieldState _currentState = FieldState.Idle;
    private GuidanceMode _guidanceMode = GuidanceMode.None;

    public event EventHandler<StateTransition> StateChanged;

    public FieldState CurrentState => _currentState;
    public GuidanceMode CurrentGuidanceMode => _guidanceMode;

    public bool CanStartField() => _currentState == FieldState.Idle;

    public bool CanEngageAutoSteer() =>
        _currentState == FieldState.GuidanceActive &&
        _guidanceMode != GuidanceMode.None;

    public Result StartField(string fieldName)
    {
        if (!CanStartField())
            return Result.Failure("Cannot start field in current state");

        TransitionTo(FieldState.FieldOpen);
        return Result.Success();
    }

    public Result SetGuidanceMode(GuidanceMode mode)
    {
        if (_currentState != FieldState.FieldOpen &&
            _currentState != FieldState.Recording)
        {
            return Result.Failure("Field must be open to set guidance");
        }

        _guidanceMode = mode;

        if (mode != GuidanceMode.None)
        {
            TransitionTo(FieldState.GuidanceActive);
        }

        return Result.Success();
    }

    public Result EngageAutoSteer(double currentSpeed)
    {
        if (!CanEngageAutoSteer())
            return Result.Failure("Cannot engage AutoSteer without guidance");

        if (Math.Abs(currentSpeed) < 0.5)
            return Result.Failure("Vehicle speed too low for AutoSteer");

        TransitionTo(FieldState.AutoSteerEngaged);
        return Result.Success();
    }

    public Result DisengageAutoSteer()
    {
        if (_currentState != FieldState.AutoSteerEngaged)
            return Result.Failure("AutoSteer not engaged");

        TransitionTo(FieldState.GuidanceActive);
        return Result.Success();
    }

    private void TransitionTo(FieldState newState)
    {
        var oldState = _currentState;
        _currentState = newState;

        StateChanged?.Invoke(this, new StateTransition
        {
            From = oldState,
            To = newState,
            GuidanceMode = _guidanceMode
        });
    }
}

// ViewModels/FieldOperationViewModel.cs
public class FieldOperationViewModel : ReactiveObject
{
    private readonly IFieldOperationStateMachine _stateMachine;

    public bool CanStartField => _stateMachine.CanStartField();
    public bool CanEngageAutoSteer => _stateMachine.CanEngageAutoSteer();

    public FieldOperationViewModel(IFieldOperationStateMachine stateMachine)
    {
        _stateMachine = stateMachine;

        _stateMachine.StateChanged += (s, transition) =>
        {
            this.RaisePropertyChanged(nameof(CanStartField));
            this.RaisePropertyChanged(nameof(CanEngageAutoSteer));

            UpdateUIForState(transition.To);
        };

        EngageAutoSteerCommand = ReactiveCommand.Create(
            () =>
            {
                var result = _stateMachine.EngageAutoSteer(CurrentSpeed);
                if (!result.Success)
                {
                    _messageService.ShowWarning(result.ErrorMessage);
                }
            },
            this.WhenAnyValue(x => x.CanEngageAutoSteer));
    }

    private void UpdateUIForState(FieldState state)
    {
        AutoSteerButtonImage = state == FieldState.AutoSteerEngaged
            ? "AutoSteerOn"
            : "AutoSteerOff";
    }
}
```

---

### Pattern 4: Cross-Form Communication Extraction
**Problem**: Forms directly accessing each other's controls and data

#### Before (Direct Form Access):
```csharp
// FormGPS.cs accessing other forms directly
private void UpdateOtherForms()
{
    // Direct form access anti-pattern
    Form formData = Application.OpenForms["FormGPSData"];
    if (formData != null)
    {
        var dataForm = formData as FormGPSData;
        dataForm.lblLatitude.Text = latitude.ToString("F7");
        dataForm.lblLongitude.Text = longitude.ToString("F7");
        dataForm.lblAltitude.Text = altitude.ToString("F1");
    }

    Form formSteer = Application.OpenForms["FormSteer"];
    if (formSteer != null)
    {
        var steerForm = formSteer as FormSteer;
        steerForm.SetSteerAngle(steerAngle);
        steerForm.SetCrossTrackError(distanceFromLine);
        steerForm.UpdateGraph();
    }
}

// FormSteer.cs
public partial class FormSteer : Form
{
    private FormGPS mf; // Direct reference to main form!

    public FormSteer(FormGPS mainForm)
    {
        mf = mainForm;
        InitializeComponent();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        // Directly accessing main form data
        lblSpeed.Text = mf.speed.ToString("F1");
        lblHeading.Text = mf.gpsHeading.ToString("F1");

        if (mf.isAutoSteerBtnOn)
        {
            btnAutoSteer.BackColor = Color.Green;
        }
    }
}
```

#### After (Message Bus Pattern):
```csharp
// Services/Messaging/MessageBus.cs
public interface IMessageBus
{
    void Publish<T>(T message) where T : class;
    IDisposable Subscribe<T>(Action<T> handler) where T : class;
}

// Using Reactive Extensions
public class MessageBus : IMessageBus
{
    private readonly Subject<object> _messages = new Subject<object>();

    public void Publish<T>(T message) where T : class
    {
        _messages.OnNext(message);
    }

    public IDisposable Subscribe<T>(Action<T> handler) where T : class
    {
        return _messages
            .OfType<T>()
            .Subscribe(handler);
    }
}

// Messages/GpsDataUpdated.cs
public class GpsDataUpdated
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public double Altitude { get; init; }
    public double Speed { get; init; }
    public double Heading { get; init; }
    public DateTime Timestamp { get; init; }
}

// Messages/SteeringDataUpdated.cs
public class SteeringDataUpdated
{
    public double SteerAngle { get; init; }
    public double CrossTrackError { get; init; }
    public bool IsAutoSteerEngaged { get; init; }
}

// Services/GPS/GpsService.cs
public class GpsService : IGpsService
{
    private readonly IMessageBus _messageBus;

    public GpsService(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    public void ProcessGpsData(NmeaSentence sentence)
    {
        // Process GPS data...
        var position = CalculatePosition(sentence);

        // Publish update for any interested subscribers
        _messageBus.Publish(new GpsDataUpdated
        {
            Latitude = position.Latitude,
            Longitude = position.Longitude,
            Altitude = position.Altitude,
            Speed = position.Speed,
            Heading = position.Heading,
            Timestamp = DateTime.Now
        });
    }
}

// ViewModels/GpsDataViewModel.cs
public class GpsDataViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    public string Latitude { get; private set; }
    public string Longitude { get; private set; }
    public string Altitude { get; private set; }

    public GpsDataViewModel(IMessageBus messageBus)
    {
        // Subscribe to GPS updates
        messageBus.Subscribe<GpsDataUpdated>(data =>
        {
            // Update on UI thread
            RxApp.MainThreadScheduler.Schedule(() =>
            {
                Latitude = data.Latitude.ToString("F7");
                Longitude = data.Longitude.ToString("F7");
                Altitude = data.Altitude.ToString("F1");

                this.RaisePropertyChanged(nameof(Latitude));
                this.RaisePropertyChanged(nameof(Longitude));
                this.RaisePropertyChanged(nameof(Altitude));
            });
        }).DisposeWith(_disposables);
    }

    public void Dispose() => _disposables.Dispose();
}

// ViewModels/SteerViewModel.cs
public class SteerViewModel : ReactiveObject, IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    public double SteerAngle { get; private set; }
    public double CrossTrackError { get; private set; }
    public bool IsAutoSteerOn { get; private set; }

    public SteerViewModel(IMessageBus messageBus)
    {
        // Subscribe to both GPS and Steering updates
        messageBus.Subscribe<GpsDataUpdated>(data =>
        {
            Speed = data.Speed;
            Heading = data.Heading;
            this.RaisePropertyChanged(nameof(Speed));
            this.RaisePropertyChanged(nameof(Heading));
        }).DisposeWith(_disposables);

        messageBus.Subscribe<SteeringDataUpdated>(data =>
        {
            SteerAngle = data.SteerAngle;
            CrossTrackError = data.CrossTrackError;
            IsAutoSteerOn = data.IsAutoSteerEngaged;

            this.RaisePropertyChanged(nameof(SteerAngle));
            this.RaisePropertyChanged(nameof(CrossTrackError));
            this.RaisePropertyChanged(nameof(IsAutoSteerOn));
        }).DisposeWith(_disposables);
    }

    public void Dispose() => _disposables.Dispose();
}
```

---

### Pattern 5: OpenGL Rendering Calculation Extraction
**Problem**: Business calculations performed during rendering

#### Before (Calculations in Paint Event):
```csharp
// FormGPS OpenGL.Designer.cs
private void oglMain_Paint(object sender, PaintEventArgs e)
{
    // Business logic mixed with rendering
    GL.Clear(ClearBufferMask.ColorBufferBit);

    // PROBLEM: Calculation during render
    double toolDistance = Math.Sqrt(
        (toolPos.easting - pivotAxlePos.easting) * (toolPos.easting - pivotAxlePos.easting) +
        (toolPos.northing - pivotAxlePos.northing) * (toolPos.northing - pivotAxlePos.northing));

    // PROBLEM: State decision in render loop
    if (toolDistance > 10 && isAutoSteerBtnOn)
    {
        isOutOfBounds = true;
        DisableAutoSteer(); // Business action in render!
    }

    // PROBLEM: Visibility calculation
    for (int i = 0; i < patches.Count; i++)
    {
        if (IsInFrustum(patches[i].center))
        {
            visiblePatches++;
            DrawPatch(patches[i]);
        }
    }

    // PROBLEM: Text calculation for display
    string speedText = (speed * 3.6).ToString("F1") + " km/h";
    DrawText(speedText, 10, 10);
}
```

#### After (Pre-calculated Render State):
```csharp
// Services/Rendering/RenderStateService.cs
public class RenderStateService : IRenderStateService
{
    private readonly object _lock = new object();
    private RenderState _currentState;

    public event EventHandler<RenderState> StateUpdated;

    public void UpdateRenderState(
        Position vehiclePos,
        Position toolPos,
        double speed,
        bool isAutoSteerOn)
    {
        var newState = new RenderState
        {
            VehiclePosition = vehiclePos,
            ToolPosition = toolPos,
            ToolDistance = CalculateDistance(vehiclePos, toolPos),
            IsOutOfBounds = DetermineOutOfBounds(vehiclePos, toolPos),
            SpeedKmh = speed * 3.6,
            SpeedText = FormatSpeed(speed),
            Timestamp = DateTime.Now
        };

        lock (_lock)
        {
            _currentState = newState;
        }

        StateUpdated?.Invoke(this, newState);
    }

    public RenderState GetCurrentState()
    {
        lock (_lock)
        {
            return _currentState;
        }
    }

    private double CalculateDistance(Position p1, Position p2)
    {
        var dx = p2.Easting - p1.Easting;
        var dy = p2.Northing - p1.Northing;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private bool DetermineOutOfBounds(Position vehicle, Position tool)
    {
        // Business logic extracted from render
        var distance = CalculateDistance(vehicle, tool);
        return distance > 10.0;
    }

    private string FormatSpeed(double metersPerSecond)
    {
        var kmh = metersPerSecond * 3.6;
        return $"{kmh:F1} km/h";
    }
}

// Services/Rendering/VisibilityCullingService.cs
public class VisibilityCullingService : IVisibilityCullingService
{
    public VisibilityResult CalculateVisibility(
        IEnumerable<Patch> patches,
        Frustum frustum)
    {
        var result = new VisibilityResult();

        foreach (var patch in patches)
        {
            if (IsInFrustum(patch.Center, frustum))
            {
                result.VisiblePatches.Add(patch);
            }
        }

        result.TotalCount = patches.Count();
        result.VisibleCount = result.VisiblePatches.Count;

        return result;
    }

    private bool IsInFrustum(Position point, Frustum frustum)
    {
        // Pure mathematical frustum culling
        foreach (var plane in frustum.Planes)
        {
            if (plane.DistanceTo(point) < 0)
                return false;
        }
        return true;
    }
}

// UI/Rendering/OpenGLRenderer.cs
public class OpenGLRenderer : IRenderer
{
    private readonly IRenderStateService _renderState;
    private readonly IVisibilityCullingService _culling;

    public OpenGLRenderer(
        IRenderStateService renderState,
        IVisibilityCullingService culling)
    {
        _renderState = renderState;
        _culling = culling;
    }

    public void Render()
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);

        // Get pre-calculated state
        var state = _renderState.GetCurrentState();

        // Just render, no calculations
        DrawVehicle(state.VehiclePosition);
        DrawTool(state.ToolPosition);

        // Use pre-calculated visibility
        var visibility = _culling.GetCurrentVisibility();
        foreach (var patch in visibility.VisiblePatches)
        {
            DrawPatch(patch);
        }

        // Use pre-formatted text
        DrawText(state.SpeedText, 10, 10);

        if (state.IsOutOfBounds)
        {
            DrawWarning("OUT OF BOUNDS", 100, 100);
        }
    }

    private void DrawVehicle(Position pos)
    {
        // Pure rendering code, no logic
        GL.PushMatrix();
        GL.Translate(pos.Easting, pos.Northing, 0);
        // Draw vehicle geometry...
        GL.PopMatrix();
    }
}
```

---

### Pattern 6: Global Variable Elimination
**Problem**: Static variables and singletons holding state

#### Before (Global Static State):
```csharp
// FormGPS.cs and various other files
public static class Globals
{
    public static double ToolWidth = 3.0;
    public static int Sections = 5;
    public static bool IsMetric = true;
    public static string FieldName = "";
    public static List<Field> Fields = new List<Field>();
}

// Usage scattered everywhere
private void CalculateCoverage()
{
    double width = Globals.ToolWidth;
    if (!Globals.IsMetric)
    {
        width = width * 3.28084; // Convert to feet
    }

    for (int i = 0; i < Globals.Sections; i++)
    {
        // Use global state
    }
}
```

#### After (Dependency Injection):
```csharp
// Configuration/ToolConfiguration.cs
public class ToolConfiguration
{
    public double Width { get; set; }
    public int SectionCount { get; set; }
    public double[] SectionWidths { get; set; }

    public double GetTotalWidth() => SectionWidths?.Sum() ?? Width;
}

// Configuration/UserPreferences.cs
public class UserPreferences
{
    public UnitSystem Units { get; set; } = UnitSystem.Metric;
    public Language Language { get; set; } = Language.English;
    public Theme Theme { get; set; } = Theme.Dark;
}

// Services/Coverage/CoverageCalculator.cs
public class CoverageCalculator : ICoverageCalculator
{
    private readonly ToolConfiguration _toolConfig;
    private readonly IUnitConverter _unitConverter;

    // Inject dependencies instead of using globals
    public CoverageCalculator(
        ToolConfiguration toolConfig,
        IUnitConverter unitConverter)
    {
        _toolConfig = toolConfig;
        _unitConverter = unitConverter;
    }

    public CoverageResult Calculate(Position position, bool[] sectionStates)
    {
        var result = new CoverageResult();

        for (int i = 0; i < _toolConfig.SectionCount; i++)
        {
            if (sectionStates[i])
            {
                var width = _toolConfig.SectionWidths[i];
                var area = CalculateSectionArea(position, width);
                result.AddSectionCoverage(i, area);
            }
        }

        return result;
    }
}

// DI Registration
services.AddSingleton<ToolConfiguration>(provider =>
{
    var config = new ToolConfiguration();
    // Load from settings
    return config;
});

services.AddSingleton<UserPreferences>();
services.AddScoped<ICoverageCalculator, CoverageCalculator>();
```

---

## Advanced Patterns

### Pattern 7: Async Operation Extraction
**Problem**: Blocking UI thread with long operations

#### Before (Blocking UI):
```csharp
private void btnLoadField_Click(object sender, EventArgs e)
{
    this.Enabled = false;
    Cursor = Cursors.WaitCursor;

    // Blocking file I/O
    string[] lines = File.ReadAllLines(fileName);

    // Blocking processing
    foreach (string line in lines)
    {
        ProcessBoundaryPoint(line);
        Application.DoEvents(); // Terrible practice!
    }

    // Blocking calculation
    CalculateFieldArea();

    this.Enabled = true;
    Cursor = Cursors.Default;
}
```

#### After (Async Service):
```csharp
// Services/Field/FieldLoadingService.cs
public class FieldLoadingService : IFieldLoadingService
{
    public async Task<Field> LoadFieldAsync(
        string path,
        IProgress<int> progress = null,
        CancellationToken cancellationToken = default)
    {
        var field = new Field();

        // Async file I/O
        var lines = await File.ReadAllLinesAsync(path, cancellationToken);

        // Process in background
        await Task.Run(() =>
        {
            for (int i = 0; i < lines.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ProcessLine(lines[i], field);

                // Report progress
                progress?.Report((i + 1) * 100 / lines.Length);
            }

            field.Area = CalculateArea(field.Boundary);
        }, cancellationToken);

        return field;
    }
}

// ViewModels/FieldViewModel.cs
public class FieldViewModel : ReactiveObject
{
    private readonly IFieldLoadingService _fieldLoader;
    private readonly CancellationTokenSource _cts = new();

    public ReactiveCommand<string, Unit> LoadFieldCommand { get; }

    public FieldViewModel(IFieldLoadingService fieldLoader)
    {
        _fieldLoader = fieldLoader;

        LoadFieldCommand = ReactiveCommand.CreateFromTask<string>(
            async (path) =>
            {
                IsLoading = true;
                LoadingProgress = 0;

                try
                {
                    var progress = new Progress<int>(value =>
                    {
                        LoadingProgress = value;
                    });

                    var field = await _fieldLoader.LoadFieldAsync(
                        path,
                        progress,
                        _cts.Token);

                    CurrentField = field;
                }
                catch (OperationCanceledException)
                {
                    // User cancelled
                }
                finally
                {
                    IsLoading = false;
                }
            });
    }
}
```

---

### Pattern 8: Configuration Extraction
**Problem**: Settings scattered across Properties.Settings.Default

#### Before (Settings Everywhere):
```csharp
private void LoadSettings()
{
    // Scattered throughout codebase
    vehicle.wheelbase = Properties.Settings.Default.setVehicle_wheelbase;
    vehicle.antennaHeight = Properties.Settings.Default.setVehicle_antennaHeight;

    if (Properties.Settings.Default.setMenu_isMetric)
    {
        // Metric
    }

    soundManager.isOn = Properties.Settings.Default.setSound_isOn;
}

private void SaveSettings()
{
    Properties.Settings.Default.setVehicle_wheelbase = vehicle.wheelbase;
    Properties.Settings.Default.Save();
}
```

#### After (Configuration Service):
```csharp
// Configuration/IConfigurationService.cs
public interface IConfigurationService
{
    T Get<T>(string key);
    void Set<T>(string key, T value);
    Task SaveAsync();
    event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;
}

// Configuration/Models/VehicleSettings.cs
public class VehicleSettings
{
    public double Wheelbase { get; set; }
    public double AntennaHeight { get; set; }
    public double AntennaOffset { get; set; }
    public double TurnRadius { get; set; }
}

// Configuration/ConfigurationService.cs
public class ConfigurationService : IConfigurationService
{
    private readonly string _configPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private Dictionary<string, object> _settings = new();

    public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

    public ConfigurationService(string configPath)
    {
        _configPath = configPath;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        LoadConfiguration();
    }

    public T Get<T>(string key)
    {
        if (_settings.TryGetValue(key, out var value))
        {
            return JsonSerializer.Deserialize<T>(
                JsonSerializer.Serialize(value, _jsonOptions),
                _jsonOptions);
        }

        return default(T);
    }

    public void Set<T>(string key, T value)
    {
        var oldValue = _settings.ContainsKey(key) ? _settings[key] : null;
        _settings[key] = value;

        ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
        {
            Key = key,
            OldValue = oldValue,
            NewValue = value
        });
    }

    public async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(_settings, _jsonOptions);
        await File.WriteAllTextAsync(_configPath, json);
    }

    private void LoadConfiguration()
    {
        if (File.Exists(_configPath))
        {
            var json = File.ReadAllText(_configPath);
            _settings = JsonSerializer.Deserialize<Dictionary<string, object>>(
                json, _jsonOptions) ?? new();
        }
    }
}

// Usage in ViewModel
public class VehicleConfigViewModel : ReactiveObject
{
    private readonly IConfigurationService _config;
    private VehicleSettings _settings;

    public VehicleConfigViewModel(IConfigurationService config)
    {
        _config = config;
        _settings = _config.Get<VehicleSettings>("Vehicle") ?? new();

        SaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            _config.Set("Vehicle", _settings);
            await _config.SaveAsync();
        });
    }

    public double Wheelbase
    {
        get => _settings.Wheelbase;
        set
        {
            _settings.Wheelbase = value;
            this.RaisePropertyChanged();
        }
    }
}
```

---

## Testing Patterns

### Pattern 9: Testable Service Extraction

#### Before (Untestable):
```csharp
public partial class FormGPS
{
    private void CalculateSteerAngle()
    {
        // Direct hardware access
        SerialPort port = new SerialPort("COM3");
        port.Open();

        // Direct file system
        File.WriteAllText("log.txt", "Calculating...");

        // Complex calculation with dependencies
        double angle = (crossTrackError * 0.9 + headingError * 0.5) *
                      vehicle.steerGain;

        // Direct UI update
        lblSteerAngle.Text = angle.ToString();

        // Direct hardware write
        port.WriteLine($"STEER,{angle}");
    }
}
```

#### After (Fully Testable):
```csharp
// Services/Steering/SteerAngleCalculator.cs
public interface ISteerAngleCalculator
{
    double Calculate(SteerInput input, SteerParameters parameters);
}

public class SteerAngleCalculator : ISteerAngleCalculator
{
    // Pure function, easily testable
    public double Calculate(SteerInput input, SteerParameters parameters)
    {
        var xteComponent = input.CrossTrackError * parameters.CrossTrackGain;
        var headingComponent = input.HeadingError * parameters.HeadingGain;
        var rawAngle = xteComponent + headingComponent;

        return LimitAngle(rawAngle * parameters.SteerGain, parameters.MaxSteerAngle);
    }

    private double LimitAngle(double angle, double max)
    {
        return Math.Max(-max, Math.Min(max, angle));
    }
}

// Unit Test
[TestFixture]
public class SteerAngleCalculatorTests
{
    private SteerAngleCalculator _calculator;

    [SetUp]
    public void Setup()
    {
        _calculator = new SteerAngleCalculator();
    }

    [Test]
    public void Calculate_WithZeroErrors_ReturnsZero()
    {
        var input = new SteerInput
        {
            CrossTrackError = 0,
            HeadingError = 0
        };

        var parameters = new SteerParameters
        {
            CrossTrackGain = 0.9,
            HeadingGain = 0.5,
            SteerGain = 1.0,
            MaxSteerAngle = 30
        };

        var result = _calculator.Calculate(input, parameters);

        Assert.That(result, Is.EqualTo(0));
    }

    [Test]
    public void Calculate_ExceedsMaxAngle_LimitsToMax()
    {
        var input = new SteerInput
        {
            CrossTrackError = 100,
            HeadingError = 100
        };

        var parameters = new SteerParameters
        {
            CrossTrackGain = 1,
            HeadingGain = 1,
            SteerGain = 1,
            MaxSteerAngle = 30
        };

        var result = _calculator.Calculate(input, parameters);

        Assert.That(result, Is.EqualTo(30));
    }
}
```

---

## Anti-Patterns to Avoid

### Anti-Pattern 1: Service Doing UI Work
```csharp
// ❌ BAD - Service knows about UI
public class BadGpsService
{
    public void UpdatePosition(Label label, TextBox textBox)
    {
        label.Text = "GPS Active";
        textBox.Text = _position.ToString();
    }
}

// ✅ GOOD - Service returns data, UI decides presentation
public class GoodGpsService
{
    public event EventHandler<PositionData> PositionUpdated;

    public void UpdatePosition(GpsData data)
    {
        var position = CalculatePosition(data);
        PositionUpdated?.Invoke(this, position);
    }
}
```

### Anti-Pattern 2: Circular Dependencies
```csharp
// ❌ BAD - Circular dependency
public class FieldService
{
    private GuidanceService _guidance;
    public FieldService(GuidanceService guidance) { }
}

public class GuidanceService
{
    private FieldService _field;
    public GuidanceService(FieldService field) { }
}

// ✅ GOOD - Use events or mediator
public class FieldService
{
    public event EventHandler<Field> FieldChanged;
}

public class GuidanceService
{
    public GuidanceService(IMessageBus bus)
    {
        bus.Subscribe<FieldChanged>(OnFieldChanged);
    }
}
```

### Anti-Pattern 3: Large Service Classes
```csharp
// ❌ BAD - God service doing everything
public class FieldService
{
    public void LoadField() { }
    public void SaveField() { }
    public void CalculateArea() { }
    public void DrawBoundary() { }
    public void CheckCollisions() { }
    // 50 more methods...
}

// ✅ GOOD - Focused services
public class FieldIOService { }
public class FieldCalculationService { }
public class BoundaryService { }
public class CollisionService { }
```

---

## Summary

These patterns provide concrete approaches for extracting business logic from WinForms to clean services:

1. **Timer Extraction**: Move timer logic to background services with events
2. **Calculation Extraction**: Create pure functions for mathematical operations
3. **State Machine**: Formalize state transitions in dedicated services
4. **Message Bus**: Replace direct form communication with pub/sub
5. **Render State**: Pre-calculate all values before rendering
6. **Global Elimination**: Replace statics with dependency injection
7. **Async Operations**: Use async/await for long-running operations
8. **Configuration Service**: Centralize all settings management
9. **Testability**: Design services as pure, mockable components

Each pattern maintains the original functionality while achieving:
- Separation of concerns
- Testability
- Cross-platform compatibility
- Maintainability
- Clear architecture

The key is to extract incrementally, test thoroughly, and maintain behavioral compatibility throughout the migration process.