using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using LiquidRocketWorkbench.App.ViewModels;

namespace LiquidRocketWorkbench.App;

/// <summary>
/// Hosts the application shell. Behavior and state live in its view model.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        InitializeComponent();
        Language = XmlLanguage.GetLanguage(
            CultureInfo.CurrentCulture.IetfLanguageTag);
        DataContext = viewModel;
    }
}
