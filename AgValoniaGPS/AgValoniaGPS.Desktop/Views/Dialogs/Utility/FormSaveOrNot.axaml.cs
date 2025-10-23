using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Utility;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Utility;

public partial class FormSaveOrNot : Window
{
    public FormSaveOrNot()
    {
        InitializeComponent();
    }

    public FormSaveOrNot(SaveOrNotViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += (sender, result) => Close(result);
    }
}
