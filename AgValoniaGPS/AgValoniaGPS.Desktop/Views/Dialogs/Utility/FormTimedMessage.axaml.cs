using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Utility;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Utility;

public partial class FormTimedMessage : Window
{
    public FormTimedMessage()
    {
        InitializeComponent();
    }

    public FormTimedMessage(TimedMessageViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
