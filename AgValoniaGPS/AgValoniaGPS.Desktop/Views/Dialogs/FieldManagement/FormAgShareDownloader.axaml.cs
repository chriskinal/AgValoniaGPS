using System;
using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

namespace AgValoniaGPS.Desktop.Views.Dialogs.FieldManagement;

public partial class FormAgShareDownloader : Window
{
    public FormAgShareDownloader()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is AgShareDownloaderViewModel viewModel)
        {
            viewModel.CloseRequested += (sender, result) => Close(result);
        }
    }
}
