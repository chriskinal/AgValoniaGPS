using System;
using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

namespace AgValoniaGPS.Desktop.Views.Dialogs.FieldManagement;

public partial class FormEnterFlag : Window
{
    public FormEnterFlag()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is EnterFlagViewModel viewModel)
        {
            viewModel.CloseRequested += (sender, result) => Close(result);
        }
    }
}
