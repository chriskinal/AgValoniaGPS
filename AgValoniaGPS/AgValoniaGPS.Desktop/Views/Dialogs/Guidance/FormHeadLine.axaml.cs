using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using AgValoniaGPS.ViewModels.Dialogs.Guidance;

namespace AgValoniaGPS.Desktop.Views.Dialogs.Guidance;

public partial class FormHeadLine : Window
{
    public FormHeadLine()
    {
        InitializeComponent();
        DataContextChanged += (_, _) =>
        {
            if (DataContext is HeadLineViewModel vm)
                vm.CloseRequested += (_, result) => Close(result);
        };
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}
