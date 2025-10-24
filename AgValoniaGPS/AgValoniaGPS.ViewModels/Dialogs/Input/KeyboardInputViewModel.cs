using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Input;
using AgValoniaGPS.ViewModels.Base;

namespace AgValoniaGPS.ViewModels.Dialogs.Input;

/// <summary>
/// ViewModel for text input with virtual keyboard
/// </summary>
public class KeyboardInputViewModel : DialogViewModelBase
{
    private string _inputText = string.Empty;
    private string _prompt = "Enter text:";

    public KeyboardInputViewModel()
    {
        AcceptCommand = new RelayCommand(OnAccept);
    }

    /// <summary>
    /// Gets or sets the input text
    /// </summary>
    public string InputText
    {
        get => _inputText;
        set => SetProperty(ref _inputText, value);
    }

    /// <summary>
    /// Gets or sets the prompt message
    /// </summary>
    public string Prompt
    {
        get => _prompt;
        set => SetProperty(ref _prompt, value);
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
