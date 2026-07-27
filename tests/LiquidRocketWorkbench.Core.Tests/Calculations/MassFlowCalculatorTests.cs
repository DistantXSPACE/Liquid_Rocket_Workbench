using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class MassFlowCalculatorTests
{
    [Fact]
    public void Calculate_WithVc04AndOptionalInputs_BuildsCompleteResult()
    {
        var result = MassFlowCalculator.Calculate(
            chamberPressurePascals: 8_000_000,
            throatAreaSquareMeters: 0.001963495408493621,
            gas: new GasProperties(
                ChamberTemperatureKelvin: 3500,
                SpecificHeatRatio: 1.22,
                SpecificGasConstantJoulesPerKilogramKelvin: 355),
            mixtureRatio: 3.5,
            targetMassFlowRateKilogramsPerSecond: 20,
            burnDurationSeconds: 10);

        AssertRelative(
            expected: 9.193408634242926,
            result.CalculatedMassFlowRateKilogramsPerSecond);
        AssertRelative(
            expected: 7.150428937744498,
            result.OxidizerMassFlowRateKilogramsPerSecond);
        AssertRelative(
            expected: 2.042979696498428,
            result.FuelMassFlowRateKilogramsPerSecond);
        Assert.Equal(20, result.TargetMassFlowRateKilogramsPerSecond);
        AssertRelative(
            expected: 10.806591365757074,
            result.AbsoluteTargetDifferenceKilogramsPerSecond!.Value);
        AssertRelative(
            expected: 1.1754716662441702,
            result.RelativeTargetDifference!.Value);
        AssertRelative(
            expected: 91.93408634242927,
            result.PropellantMassConsumedKilograms!.Value);
    }

    [Fact]
    public void Calculate_WithoutOptionalInputs_LeavesComparisonsEmpty()
    {
        var result = MassFlowCalculator.Calculate(
            chamberPressurePascals: 1_000_000,
            throatAreaSquareMeters: 0.01,
            gas: CreateValidGas(),
            mixtureRatio: 1);

        Assert.Null(result.TargetMassFlowRateKilogramsPerSecond);
        Assert.Null(result.AbsoluteTargetDifferenceKilogramsPerSecond);
        Assert.Null(result.RelativeTargetDifference);
        Assert.Null(result.PropellantMassConsumedKilograms);
    }

    [Theory]
    [InlineData(0, null)]
    [InlineData(-1, null)]
    [InlineData(double.NaN, null)]
    [InlineData(1, 0.0)]
    [InlineData(1, -1.0)]
    [InlineData(1, double.PositiveInfinity)]
    public void Calculate_WithInvalidMixtureOrTarget_Throws(
        double mixtureRatio,
        double? targetMassFlowRate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => MassFlowCalculator.Calculate(
                chamberPressurePascals: 1_000_000,
                throatAreaSquareMeters: 0.01,
                gas: CreateValidGas(),
                mixtureRatio,
                targetMassFlowRate));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Calculate_WithInvalidBurnDuration_Throws(
        double burnDuration)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => MassFlowCalculator.Calculate(
                chamberPressurePascals: 1_000_000,
                throatAreaSquareMeters: 0.01,
                gas: CreateValidGas(),
                mixtureRatio: 1,
                burnDurationSeconds: burnDuration));
    }

    [Fact]
    public void Calculate_WhenRelativeTargetDifferenceOverflows_Throws()
    {
        Assert.Throws<OverflowException>(
            () => MassFlowCalculator.Calculate(
                chamberPressurePascals: 1e-300,
                throatAreaSquareMeters: 1,
                gas: CreateValidGas(),
                mixtureRatio: 1,
                targetMassFlowRateKilogramsPerSecond: double.MaxValue));
    }

    [Fact]
    public void Calculate_WhenConsumedMassOverflows_Throws()
    {
        Assert.Throws<OverflowException>(
            () => MassFlowCalculator.Calculate(
                chamberPressurePascals: 1_000_000,
                throatAreaSquareMeters: 0.01,
                gas: CreateValidGas(),
                mixtureRatio: 1,
                burnDurationSeconds: double.MaxValue));
    }

    private static GasProperties CreateValidGas()
    {
        return new GasProperties(
            ChamberTemperatureKelvin: 300,
            SpecificHeatRatio: 1.4,
            SpecificGasConstantJoulesPerKilogramKelvin: 287.05);
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
