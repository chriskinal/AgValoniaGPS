using System;
using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

namespace AgValoniaGPS.Desktop.Views.Dialogs.FieldManagement;

public partial class FormBuildBoundaryFromTracks : Window
{
    public FormBuildBoundaryFromTracks()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is BuildBoundaryFromTracksViewModel viewModel)
        {
            viewModel.CloseRequested += (sender, result) => Close(result);
        }
    }
}
