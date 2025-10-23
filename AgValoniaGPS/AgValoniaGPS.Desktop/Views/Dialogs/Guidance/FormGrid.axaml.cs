using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AgValoniaGPS.ViewModels.Dialogs.Guidance;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Guidance;

public partial class FormGrid : Window
{
    public FormGrid()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is GridViewModel vm)
                vm.CloseRequested += (_, result) => Close(result);
        };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
