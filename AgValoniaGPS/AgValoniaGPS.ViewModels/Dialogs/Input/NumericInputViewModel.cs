using System;
using System.Reactive;
using System.Windows.Input;
using AgValoniaGPS.ViewModels.Base;
using ReactiveUI;

namespace AgValoniaGPS.ViewModels.Dialogs.Input;

/// <summary>
/// ViewModel for numeric input with virtual numeric keypad
/// </summary>
public class NumericInputViewModel : DialogViewModelBase
{
    private double _value;
    private string _prompt = "Enter value:";
    private double _minimum;
    private double _maximum = 1000;

    public NumericInputViewModel()
    {
        AcceptCommand = ReactiveCommand.Create(OnAccept);
    }

    /// <summary>
    /// Gets or sets the numeric value
    /// </summary>
    public double Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    /// <summary>
    /// Gets or sets the prompt message
    /// </summary>
    public string Prompt
    {
        get => _prompt;
        set => this.RaiseAndSetIfChanged(ref _prompt, value);
    }

    /// <summary>
    /// Gets or sets the minimum allowed value
    /// </summary>
    public double Minimum
    {
        get => _minimum;
        set => this.RaiseAndSetIfChanged(ref _minimum, value);
    }

    /// <summary>
    /// Gets or sets the maximum allowed value
    /// </summary>
    public double Maximum
    {
        get => _maximum;
        set => this.RaiseAndSetIfChanged(ref _maximum, value);
    }

    /// <summary>
    /// Gets the command to accept input
    /// </summary>
    public ICommand AcceptCommand { get; }

    private void OnAccept()
    {
        DialogResult = true;
        RequestClose(true);
    }
}
