using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AgValoniaGPS.ViewModels.Dialogs.Guidance;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Guidance;

public partial class FormSmoothAB : Window
{
    public FormSmoothAB()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is SmoothABViewModel vm)
                vm.CloseRequested += (_, result) => Close(result);
        };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
