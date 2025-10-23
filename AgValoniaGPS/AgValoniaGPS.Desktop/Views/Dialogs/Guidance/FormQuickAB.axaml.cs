using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AgValoniaGPS.ViewModels.Dialogs.Guidance;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Guidance;

public partial class FormQuickAB : Window
{
    public FormQuickAB()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is QuickABViewModel vm)
                vm.CloseRequested += (_, result) => Close(result);
        };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
