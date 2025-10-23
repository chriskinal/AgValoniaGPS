using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AgValoniaGPS.ViewModels.Dialogs.Guidance;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Guidance;

/// <summary>
/// Code-behind for FormRecordName dialog.
/// Recording naming dialog for saved guidance paths.
/// </summary>
public partial class FormRecordName : Window
{
    public FormRecordName()
    {
        InitializeComponent();

        // Subscribe to CloseRequested event if ViewModel is set
        DataContextChanged += (_, _) =>
        {
            if (DataContext is RecordNameViewModel viewModel)
            {
                viewModel.CloseRequested += (_, result) => Close(result);
            }
        };
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
