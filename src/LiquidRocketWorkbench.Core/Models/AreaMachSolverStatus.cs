namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Describes the outcome of a bounded area-Mach solve.
/// </summary>
public enum AreaMachSolverStatus
{
    Converged,
    RootNotBracketed,
    IterationLimitReached,
}
