using Avalonia.Controls;
using Avalonia.Interactivity;
using AgValoniaGPS.ViewModels.Dialogs.Input;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Input
{
    /// <summary>
    /// Numeric input dialog using touch-friendly NumericKeypad control.
    /// </summary>
    public partial class FormNumeric : Window
    {
        public FormNumeric()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handle Enter key pressed on NumericKeypad - accept the dialog.
        /// </summary>
        private void OnNumericKeypadEnter(object? sender, RoutedEventArgs e)
        {
            if (DataContext is NumericInputViewModel viewModel)
            {
                if (viewModel.AcceptCommand.CanExecute(null))
                {
                    viewModel.AcceptCommand.Execute(null);
                }
            }
        }
    }
}
