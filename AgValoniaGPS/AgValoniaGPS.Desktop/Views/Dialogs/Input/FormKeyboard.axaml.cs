using Avalonia.Controls;
using Avalonia.Interactivity;
using AgValoniaGPS.ViewModels.Dialogs.Input;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Input
{
    /// <summary>
    /// Virtual keyboard input dialog for text entry using touch-friendly on-screen keyboard.
    /// </summary>
    public partial class FormKeyboard : Window
    {
        public FormKeyboard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handle Enter key pressed on VirtualKeyboard - accept the dialog.
        /// </summary>
        private void OnVirtualKeyboardEnter(object? sender, RoutedEventArgs e)
        {
            if (DataContext is KeyboardInputViewModel viewModel)
            {
                if (viewModel.AcceptCommand.CanExecute(null))
                {
                    viewModel.AcceptCommand.Execute(null);
                }
            }
        }
    }
}
