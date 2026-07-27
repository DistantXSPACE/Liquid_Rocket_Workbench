using System.Windows;
using LiquidRocketWorkbench.App.ViewModels;
using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Validation;

namespace LiquidRocketWorkbench.App;

/// <summary>
/// Application composition root.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IEnginePerformanceCalculator performanceCalculator =
            new EnginePerformanceCalculator();
        var inputs = new EngineInputViewModel(new EngineInputsValidator());
        var viewModel = new MainWindowViewModel(
            performanceCalculator,
            inputs);
        var window = new MainWindow(viewModel);

        MainWindow = window;
        window.Show();
    }
}
