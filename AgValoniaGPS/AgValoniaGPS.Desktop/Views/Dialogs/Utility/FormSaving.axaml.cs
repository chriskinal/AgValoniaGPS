using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.Utility;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Utility;

public partial class FormSaving : Window
{
    public FormSaving()
    {
        InitializeComponent();
    }

    public FormSaving(SavingViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
