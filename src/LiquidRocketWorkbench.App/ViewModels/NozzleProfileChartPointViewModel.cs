namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// One fixed-canvas point used by a responsive profile-chart Viewbox.
/// </summary>
public sealed record NozzleProfileChartPointViewModel(
    double X,
    double Y);
