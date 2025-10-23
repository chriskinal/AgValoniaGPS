using System;
using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

namespace AgValoniaGPS.Desktop.Views.Dialogs.FieldManagement;

public partial class FormBoundaryPlayer : Window
{
    public FormBoundaryPlayer()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is BoundaryPlayerViewModel viewModel)
        {
            viewModel.CloseRequested += (sender, result) => Close(result);
        }
    }
}
