using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using AgValoniaGPS.ViewModels.Base;

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
        PanUpCommand = new RelayCommand(PanUp);
        PanDownCommand = new RelayCommand(PanDown);
        PanLeftCommand = new RelayCommand(PanLeft);
        PanRightCommand = new RelayCommand(PanRight);
        PanUpLeftCommand = new RelayCommand(PanUpLeft);
        PanUpRightCommand = new RelayCommand(PanUpRight);
        PanDownLeftCommand = new RelayCommand(PanDownLeft);
        PanDownRightCommand = new RelayCommand(PanDownRight);
        CenterCommand = new RelayCommand(Center);
        ResetCommand = new RelayCommand(Reset);
    }

    /// <summary>
    /// Gets or sets the horizontal pan offset in pixels.
    /// </summary>
    public double PanOffsetX
    {
        get => _panOffsetX;
        set => SetProperty(ref _panOffsetX, value);
    }

    /// <summary>
    /// Gets or sets the vertical pan offset in pixels.
    /// </summary>
    public double PanOffsetY
    {
        get => _panOffsetY;
        set => SetProperty(ref _panOffsetY, value);
    }

    /// <summary>
    /// Gets or sets the pan step size in pixels.
    /// </summary>
    public double PanStep
    {
        get => _panStep;
        set => SetProperty(ref _panStep, value);
    }

    /// <summary>
    /// Gets the command to pan up.
    /// </summary>
    public ICommand PanUpCommand { get; }

    /// <summary>
    /// Gets the command to pan down.
    /// </summary>
    public ICommand PanDownCommand { get; }

    /// <summary>
    /// Gets the command to pan left.
    /// </summary>
    public ICommand PanLeftCommand { get; }

    /// <summary>
    /// Gets the command to pan right.
    /// </summary>
    public ICommand PanRightCommand { get; }

    /// <summary>
    /// Gets the command to pan up-left diagonally.
    /// </summary>
    public ICommand PanUpLeftCommand { get; }

    /// <summary>
    /// Gets the command to pan up-right diagonally.
    /// </summary>
    public ICommand PanUpRightCommand { get; }

    /// <summary>
    /// Gets the command to pan down-left diagonally.
    /// </summary>
    public ICommand PanDownLeftCommand { get; }

    /// <summary>
    /// Gets the command to pan down-right diagonally.
    /// </summary>
    public ICommand PanDownRightCommand { get; }

    /// <summary>
    /// Gets the command to return to center.
    /// </summary>
    public ICommand CenterCommand { get; }

    /// <summary>
    /// Gets the command to reset all settings.
    /// </summary>
    public ICommand ResetCommand { get; }

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
