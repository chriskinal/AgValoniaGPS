using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AgValoniaGPS.ViewModels.Dialogs.Guidance;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Guidance;

public partial class FormTramLine : Window
{
    public FormTramLine()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is TramLineViewModel vm)
                vm.CloseRequested += (_, result) => Close(result);
        };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
