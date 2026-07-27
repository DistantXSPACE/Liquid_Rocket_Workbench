using LiquidRocketWorkbench.Core.Diagnostics;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Solves EQ-FLOW-02 from <c>docs/references.md</c> on the supersonic branch.
/// </summary>
public static class SupersonicAreaMachSolver
{
    public static AreaMachSolverResult Solve(
        double expansionRatio,
        double specificHeatRatio,
        AreaMachSolverOptions? options = null)
    {
        CalculationGuard.RequirePositiveFinite(
            expansionRatio,
            nameof(expansionRatio));
        ValidateSpecificHeatRatio(specificHeatRatio);

        if (expansionRatio < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expansionRatio),
                expansionRatio,
                "Expansion ratio must be greater than or equal to one.");
        }

        var solverOptions = options ?? new AreaMachSolverOptions();
        ValidateOptions(solverOptions);

        var throatResidual = expansionRatio - 1;
        if (throatResidual / expansionRatio
            <= CalculationTolerances.AreaRatioRelative)
        {
            return AreaMachSolverResult.Converged(
                machNumber: 1,
                iterations: 0,
                relativeAreaRatioResidual: throatResidual / expansionRatio);
        }

        var lowerMachNumber = 1.0;
        var upperMachNumber = solverOptions.MaximumMachNumber;
        var targetLogAreaRatio = Math.Log(expansionRatio);
        var upperLogAreaRatio = CalculateLogAreaRatio(
            upperMachNumber,
            specificHeatRatio);
        var upperResidual = CalculateRelativeResidual(
            upperLogAreaRatio,
            targetLogAreaRatio);

        if (upperResidual <= CalculationTolerances.AreaRatioRelative)
        {
            return AreaMachSolverResult.Converged(
                upperMachNumber,
                iterations: 0,
                upperResidual);
        }

        if (upperLogAreaRatio < targetLogAreaRatio)
        {
            return AreaMachSolverResult.Failed(
                AreaMachSolverStatus.RootNotBracketed,
                iterations: 0,
                upperResidual,
                new ValidationIssue(
                    CalculationDiagnosticCodes.AreaMachRootNotBracketed,
                    ValidationSeverity.Error,
                    $"The supersonic area-Mach root is above the configured "
                        + $"Mach limit of {upperMachNumber:R}."));
        }

        var lastResidual = upperResidual;

        for (var iteration = 1;
            iteration <= solverOptions.MaximumIterations;
            iteration++)
        {
            var midpointMachNumber =
                lowerMachNumber
                + ((upperMachNumber - lowerMachNumber) / 2);

            if (midpointMachNumber == lowerMachNumber
                || midpointMachNumber == upperMachNumber)
            {
                return CreateIterationLimitFailure(
                    iteration - 1,
                    lastResidual);
            }

            var midpointLogAreaRatio = CalculateLogAreaRatio(
                midpointMachNumber,
                specificHeatRatio);
            lastResidual = CalculateRelativeResidual(
                midpointLogAreaRatio,
                targetLogAreaRatio);

            if (lastResidual <= CalculationTolerances.AreaRatioRelative)
            {
                return AreaMachSolverResult.Converged(
                    midpointMachNumber,
                    iteration,
                    lastResidual);
            }

            if (midpointLogAreaRatio < targetLogAreaRatio)
            {
                lowerMachNumber = midpointMachNumber;
            }
            else
            {
                upperMachNumber = midpointMachNumber;
            }
        }

        return CreateIterationLimitFailure(
            solverOptions.MaximumIterations,
            lastResidual);
    }

    private static double CalculateLogAreaRatio(
        double machNumber,
        double specificHeatRatio)
    {
        var gammaDelta = specificHeatRatio - 1;
        var exponent = 0.5 + (1 / gammaDelta);
        var normalizedCoefficient =
            (gammaDelta / specificHeatRatio)
            / (1 + (1 / specificHeatRatio));
        var normalizedFlowDelta =
            normalizedCoefficient * ((machNumber * machNumber) - 1);

        return -Math.Log(machNumber)
            + (exponent * CalculationMath.LogOnePlus(
                normalizedFlowDelta));
    }

    private static double CalculateRelativeResidual(
        double actualLogAreaRatio,
        double targetLogAreaRatio)
    {
        return Math.Abs(
            Math.Exp(actualLogAreaRatio - targetLogAreaRatio) - 1);
    }

    private static AreaMachSolverResult CreateIterationLimitFailure(
        int iterations,
        double relativeAreaRatioResidual)
    {
        return AreaMachSolverResult.Failed(
            AreaMachSolverStatus.IterationLimitReached,
            iterations,
            relativeAreaRatioResidual,
            new ValidationIssue(
                CalculationDiagnosticCodes.AreaMachIterationLimitReached,
                ValidationSeverity.Error,
                "The supersonic area-Mach solver did not converge within the "
                    + "configured iteration limit."));
    }

    private static void ValidateSpecificHeatRatio(double specificHeatRatio)
    {
        CalculationGuard.RequireGreaterThanOneFinite(
            specificHeatRatio,
            nameof(specificHeatRatio));
    }

    private static void ValidateOptions(AreaMachSolverOptions options)
    {
        if (!double.IsFinite(options.MaximumMachNumber)
            || options.MaximumMachNumber <= 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.MaximumMachNumber,
                "Maximum Mach number must be finite and greater than one.");
        }

        if (options.MaximumIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.MaximumIterations,
                "Maximum iterations must be greater than zero.");
        }
    }
}
