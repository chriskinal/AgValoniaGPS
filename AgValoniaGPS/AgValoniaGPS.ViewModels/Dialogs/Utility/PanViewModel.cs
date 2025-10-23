using System;
using System.Reactive;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Utility;

/// <summary>
/// ViewModel for manual panning control with directional buttons.
/// Provides commands for panning in 8 directions and returning to center.
/// </summary>
public class PanViewModel : DialogViewModelBase
{
    private double _panOffsetX;
    private double _panOffsetY;
    private double _panStep = 100.0; // pixels per step

    /// <summary>
    /// Initializes a new instance of the <see cref="PanViewModel"/> class.
    /// </summary>
    public PanViewModel()
    {
        PanUpCommand = ReactiveCommand.Create(PanUp);
        PanDownCommand = ReactiveCommand.Create(PanDown);
        PanLeftCommand = ReactiveCommand.Create(PanLeft);
        PanRightCommand = ReactiveCommand.Create(PanRight);
        PanUpLeftCommand = ReactiveCommand.Create(PanUpLeft);
        PanUpRightCommand = ReactiveCommand.Create(PanUpRight);
        PanDownLeftCommand = ReactiveCommand.Create(PanDownLeft);
        PanDownRightCommand = ReactiveCommand.Create(PanDownRight);
        CenterCommand = ReactiveCommand.Create(Center);
        ResetCommand = ReactiveCommand.Create(Reset);
    }

    /// <summary>
    /// Gets or sets the horizontal pan offset in pixels.
    /// </summary>
    public double PanOffsetX
    {
        get => _panOffsetX;
        set => this.RaiseAndSetIfChanged(ref _panOffsetX, value);
    }

    /// <summary>
    /// Gets or sets the vertical pan offset in pixels.
    /// </summary>
    public double PanOffsetY
    {
        get => _panOffsetY;
        set => this.RaiseAndSetIfChanged(ref _panOffsetY, value);
    }

    /// <summary>
    /// Gets or sets the pan step size in pixels.
    /// </summary>
    public double PanStep
    {
        get => _panStep;
        set => this.RaiseAndSetIfChanged(ref _panStep, value);
    }

    /// <summary>
    /// Gets the command to pan up.
    /// </summary>
    public ReactiveCommand<Unit, Unit> PanUpCommand { get; }

    /// <summary>
    /// Gets the command to pan down.
    /// </summary>
    public ReactiveCommand<Unit, Unit> PanDownCommand { get; }

    /// <summary>
    /// Gets the command to pan left.
    /// </summary>
    public ReactiveCommand<Unit, Unit> PanLeftCommand { get; }

    /// <summary>
    /// Gets the command to pan right.
    /// </summary>
    public ReactiveCommand<Unit, Unit> PanRightCommand { get; }

    /// <summary>
    /// Gets the command to pan up-left diagonally.
    /// </summary>
    public ReactiveCommand<Unit, Unit> PanUpLeftCommand { get; }

    /// <summary>
    /// Gets the command to pan up-right diagonally.
    /// </summary>
    public ReactiveCommand<Unit, Unit> PanUpRightCommand { get; }

    /// <summary>
    /// Gets the command to pan down-left diagonally.
    /// </summary>
    public ReactiveCommand<Unit, Unit> PanDownLeftCommand { get; }

    /// <summary>
    /// Gets the command to pan down-right diagonally.
    /// </summary>
    public ReactiveCommand<Unit, Unit> PanDownRightCommand { get; }

    /// <summary>
    /// Gets the command to return to center.
    /// </summary>
    public ReactiveCommand<Unit, Unit> CenterCommand { get; }

    /// <summary>
    /// Gets the command to reset all settings.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ResetCommand { get; }

    private void PanUp() => PanOffsetY -= PanStep;
    private void PanDown() => PanOffsetY += PanStep;
    private void PanLeft() => PanOffsetX -= PanStep;
    private void PanRight() => PanOffsetX += PanStep;

    private void PanUpLeft()
    {
        PanOffsetX -= PanStep;
        PanOffsetY -= PanStep;
    }

    private void PanUpRight()
    {
        PanOffsetX += PanStep;
        PanOffsetY -= PanStep;
    }

    private void PanDownLeft()
    {
        PanOffsetX -= PanStep;
        PanOffsetY += PanStep;
    }

    private void PanDownRight()
    {
        PanOffsetX += PanStep;
        PanOffsetY += PanStep;
    }

    private void Center()
    {
        PanOffsetX = 0;
        PanOffsetY = 0;
    }

    private void Reset()
    {
        Center();
        PanStep = 100.0;
    }
}
