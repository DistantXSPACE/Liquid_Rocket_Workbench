using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Diagnostics;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class SupersonicAreaMachSolverTests
{
    [Fact]
    public void Solve_WithVc02AreaRatio_ReturnsSupersonicMachTwo()
    {
        var result = SupersonicAreaMachSolver.Solve(
            expansionRatio: 1.6875,
            specificHeatRatio: 1.4);

        AssertConverged(result);
        AssertRelative(expected: 2, result.MachNumber!.Value);
        Assert.True(
            result.RelativeAreaRatioResidual
                <= CalculationTolerances.AreaRatioRelative);
    }

    [Fact]
    public void Solve_WithVc04ExpansionRatio_MatchesFixedReferenceMach()
    {
        var result = SupersonicAreaMachSolver.Solve(
            expansionRatio: 40,
            specificHeatRatio: 1.22);

        AssertConverged(result);
        AssertRelative(
            expected: 4.355537019605814,
            result.MachNumber!.Value);
    }

    [Fact]
    public void Solve_WithUnitExpansionRatio_ReturnsThroatExactly()
    {
        var result = SupersonicAreaMachSolver.Solve(
            expansionRatio: 1,
            specificHeatRatio: 1.4);

        AssertConverged(result);
        Assert.Equal(1, result.MachNumber);
        Assert.Equal(0, result.Iterations);
        Assert.Equal(0, result.RelativeAreaRatioResidual);
    }

    [Fact]
    public void Solve_WithinUnityTolerance_ReturnsThroatExactly()
    {
        var expansionRatio =
            1 + (CalculationTolerances.AreaRatioRelative / 2);

        var result = SupersonicAreaMachSolver.Solve(
            expansionRatio,
            specificHeatRatio: 1.4);

        AssertConverged(result);
        Assert.Equal(1, result.MachNumber);
        Assert.True(
            result.RelativeAreaRatioResidual
                <= CalculationTolerances.AreaRatioRelative);
    }

    [Fact]
    public void Solve_AboveUnityTolerance_ReturnsSupersonicRoot()
    {
        var expansionRatio =
            1 + (CalculationTolerances.AreaRatioRelative * 2);

        var result = SupersonicAreaMachSolver.Solve(
            expansionRatio,
            specificHeatRatio: 1.4);

        AssertConverged(result);
        Assert.True(result.MachNumber > 1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.999999)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Solve_WithInvalidExpansionRatio_Throws(double expansionRatio)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => SupersonicAreaMachSolver.Solve(
                expansionRatio,
                specificHeatRatio: 1.4));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Solve_WithInvalidSpecificHeatRatio_Throws(
        double specificHeatRatio)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => SupersonicAreaMachSolver.Solve(
                expansionRatio: 2,
                specificHeatRatio));
    }

    [Fact]
    public void Solve_WhenRootEqualsUpperBound_ReturnsConvergedBoundary()
    {
        var options = new AreaMachSolverOptions(
            MaximumMachNumber: 2,
            MaximumIterations: 100);

        var result = SupersonicAreaMachSolver.Solve(
            expansionRatio: 1.6875,
            specificHeatRatio: 1.4,
            options);

        AssertConverged(result);
        Assert.Equal(2, result.MachNumber);
        Assert.Equal(0, result.Iterations);
    }

    [Fact]
    public void Solve_WhenRootIsNotBracketed_ReturnsExplicitFailure()
    {
        var options = new AreaMachSolverOptions(
            MaximumMachNumber: 1.5,
            MaximumIterations: 100);

        var result = SupersonicAreaMachSolver.Solve(
            expansionRatio: 1.6875,
            specificHeatRatio: 1.4,
            options);

        Assert.False(result.IsConverged);
        Assert.Equal(AreaMachSolverStatus.RootNotBracketed, result.Status);
        Assert.Null(result.MachNumber);
        Assert.Equal(0, result.Iterations);
        Assert.Equal(
            CalculationDiagnosticCodes.AreaMachRootNotBracketed,
            result.Diagnostic?.Code);
        Assert.Equal(ValidationSeverity.Error, result.Diagnostic?.Severity);
    }

    [Fact]
    public void Solve_WhenIterationLimitIsReached_ReturnsExplicitFailure()
    {
        var options = new AreaMachSolverOptions(
            MaximumMachNumber: 50,
            MaximumIterations: 1);

        var result = SupersonicAreaMachSolver.Solve(
            expansionRatio: 1.6875,
            specificHeatRatio: 1.4,
            options);

        Assert.False(result.IsConverged);
        Assert.Equal(
            AreaMachSolverStatus.IterationLimitReached,
            result.Status);
        Assert.Null(result.MachNumber);
        Assert.Equal(1, result.Iterations);
        Assert.Equal(
            CalculationDiagnosticCodes.AreaMachIterationLimitReached,
            result.Diagnostic?.Code);
        Assert.Equal(ValidationSeverity.Error, result.Diagnostic?.Severity);
    }

    [Theory]
    [InlineData(1, 100)]
    [InlineData(0, 100)]
    [InlineData(double.NaN, 100)]
    [InlineData(double.PositiveInfinity, 100)]
    [InlineData(50, 0)]
    [InlineData(50, -1)]
    public void Solve_WithInvalidOptions_Throws(
        double maximumMachNumber,
        int maximumIterations)
    {
        var options = new AreaMachSolverOptions(
            maximumMachNumber,
            maximumIterations);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => SupersonicAreaMachSolver.Solve(
                expansionRatio: 2,
                specificHeatRatio: 1.4,
                options));
    }

    private static void AssertConverged(AreaMachSolverResult result)
    {
        Assert.True(result.IsConverged);
        Assert.Equal(AreaMachSolverStatus.Converged, result.Status);
        Assert.NotNull(result.MachNumber);
        Assert.Null(result.Diagnostic);
    }

    private static void AssertRelative(
        double expected,
        double actual,
        double tolerance = 1e-8)
    {
        var relativeError = Math.Abs(actual - expected) / Math.Abs(expected);

        Assert.True(
            relativeError <= tolerance,
            $"Expected {expected:R}, actual {actual:R}, "
                + $"relative error {relativeError:R}.");
    }
}
