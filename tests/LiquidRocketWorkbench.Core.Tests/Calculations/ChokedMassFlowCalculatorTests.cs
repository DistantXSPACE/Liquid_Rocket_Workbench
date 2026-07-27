using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class ChokedMassFlowCalculatorTests
{
    [Fact]
    public void Calculate_WithVc03Inputs_MatchesFixedReferenceFlow()
    {
        var massFlowRate = ChokedMassFlowCalculator.Calculate(
            chamberPressurePascals: 1_000_000,
            throatAreaSquareMeters: 0.01,
            gas: new GasProperties(
                ChamberTemperatureKelvin: 300,
                SpecificHeatRatio: 1.4,
                SpecificGasConstantJoulesPerKilogramKelvin: 287.05));

        AssertRelative(
            expected: 23.333553155106348,
            massFlowRate);
    }

    [Fact]
    public void Calculate_WithVc04Inputs_MatchesFixedReferenceFlow()
    {
        var massFlowRate = ChokedMassFlowCalculator.Calculate(
            chamberPressurePascals: 8_000_000,
            throatAreaSquareMeters: 0.001963495408493621,
            gas: new GasProperties(
                ChamberTemperatureKelvin: 3500,
                SpecificHeatRatio: 1.22,
                SpecificGasConstantJoulesPerKilogramKelvin: 355));

        AssertRelative(
            expected: 9.193408634242926,
            massFlowRate);
    }

    [Theory]
    [InlineData(0, 0.01)]
    [InlineData(-1, 0.01)]
    [InlineData(double.NaN, 0.01)]
    [InlineData(1_000_000, 0)]
    [InlineData(1_000_000, double.PositiveInfinity)]
    public void Calculate_WithInvalidScalarInput_Throws(
        double chamberPressure,
        double throatArea)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ChokedMassFlowCalculator.Calculate(
                chamberPressure,
                throatArea,
                CreateValidGas()));
    }

    [Theory]
    [InlineData(0, 1.4, 287.05)]
    [InlineData(double.NaN, 1.4, 287.05)]
    [InlineData(300, 1, 287.05)]
    [InlineData(300, double.PositiveInfinity, 287.05)]
    [InlineData(300, 1.4, 0)]
    [InlineData(300, 1.4, double.NaN)]
    public void Calculate_WithInvalidGas_Throws(
        double chamberTemperature,
        double specificHeatRatio,
        double specificGasConstant)
    {
        var gas = new GasProperties(
            chamberTemperature,
            specificHeatRatio,
            specificGasConstant);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ChokedMassFlowCalculator.Calculate(
                chamberPressurePascals: 1_000_000,
                throatAreaSquareMeters: 0.01,
                gas));
    }

    [Fact]
    public void Calculate_WithNullGas_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => ChokedMassFlowCalculator.Calculate(
                chamberPressurePascals: 1_000_000,
                throatAreaSquareMeters: 0.01,
                gas: null!));
    }

    [Fact]
    public void Calculate_WhenResultOverflows_Throws()
    {
        Assert.Throws<OverflowException>(
            () => ChokedMassFlowCalculator.Calculate(
                chamberPressurePascals: double.MaxValue,
                throatAreaSquareMeters: double.MaxValue,
                CreateValidGas()));
    }

    [Fact]
    public void Calculate_WhenResultUnderflows_Throws()
    {
        Assert.Throws<ArithmeticException>(
            () => ChokedMassFlowCalculator.Calculate(
                chamberPressurePascals: double.Epsilon,
                throatAreaSquareMeters: double.Epsilon,
                CreateValidGas()));
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
