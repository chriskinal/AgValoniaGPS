using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Utility;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Utility;

public partial class FormPan : Window
{
    public FormPan()
    {
        InitializeComponent();
    }

    public FormPan(PanViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
