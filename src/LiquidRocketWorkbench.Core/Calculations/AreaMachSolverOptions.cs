namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Defines the finite search bound and iteration limit for the supersonic
/// area-Mach solver.
/// </summary>
public sealed record AreaMachSolverOptions(
    double MaximumMachNumber = 50,
    int MaximumIterations = 100);
