using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Utility;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Utility;

public partial class Form_Help : Window
{
    public Form_Help()
    {
        InitializeComponent();
    }

    public Form_Help(HelpViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
