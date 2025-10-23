using Avalonia.Controls;
using AgValoniaGPS.ViewModels.Dialogs.FieldManagement;

namespace AgValoniaGPS.Desktop.Views.Dialogs.FieldManagement;

public partial class FormFieldData : Window
{
    public FormFieldData()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is FieldDataViewModel viewModel)
        {
            viewModel.CloseRequested += (sender, result) => Close(result);
        }
    }
}
