using LiquidRocketWorkbench.Core.Diagnostics;

namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Represents either a converged supersonic Mach number or an explicit solver
/// failure.
/// </summary>
public sealed class AreaMachSolverResult
{
    private AreaMachSolverResult(
        AreaMachSolverStatus status,
        double? machNumber,
        int iterations,
        double relativeAreaRatioResidual,
        ValidationIssue? diagnostic)
    {
        Status = status;
        MachNumber = machNumber;
        Iterations = iterations;
        RelativeAreaRatioResidual = relativeAreaRatioResidual;
        Diagnostic = diagnostic;
    }

    public AreaMachSolverStatus Status { get; }

    public bool IsConverged => Status == AreaMachSolverStatus.Converged;

    public double? MachNumber { get; }

    public int Iterations { get; }

    public double RelativeAreaRatioResidual { get; }

    public ValidationIssue? Diagnostic { get; }

    internal static AreaMachSolverResult Converged(
        double machNumber,
        int iterations,
        double relativeAreaRatioResidual)
    {
        return new AreaMachSolverResult(
            AreaMachSolverStatus.Converged,
            machNumber,
            iterations,
            relativeAreaRatioResidual,
            diagnostic: null);
    }

    internal static AreaMachSolverResult Failed(
        AreaMachSolverStatus status,
        int iterations,
        double relativeAreaRatioResidual,
        ValidationIssue diagnostic)
    {
        return new AreaMachSolverResult(
            status,
            machNumber: null,
            iterations,
            relativeAreaRatioResidual,
            diagnostic);
    }
}
