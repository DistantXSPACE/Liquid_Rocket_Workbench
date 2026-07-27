using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class CharacteristicVelocityCalculatorTests
{
    [Fact]
    public void CalculateMethods_WithVc03Inputs_MatchFixedReference()
    {
        var gas = new GasProperties(
            ChamberTemperatureKelvin: 300,
            SpecificHeatRatio: 1.4,
            SpecificGasConstantJoulesPerKilogramKelvin: 287.05);

        var ideal = CharacteristicVelocityCalculator.CalculateIdeal(gas);
        var fromMassFlow =
            CharacteristicVelocityCalculator.CalculateFromMassFlow(
                chamberPressurePascals: 1_000_000,
                throatAreaSquareMeters: 0.01,
                massFlowRateKilogramsPerSecond: 23.333553155106348);

        AssertRelative(expected: 428.56739106669596, ideal);
        AssertRelative(expected: 428.56739106669596, fromMassFlow);
        AssertRelative(ideal, fromMassFlow);
    }

    [Fact]
    public void CalculateIdeal_WithVc04Inputs_MatchesFixedReference()
    {
        var characteristicVelocity =
            CharacteristicVelocityCalculator.CalculateIdeal(
                new GasProperties(
                    ChamberTemperatureKelvin: 3500,
                    SpecificHeatRatio: 1.22,
                    SpecificGasConstantJoulesPerKilogramKelvin: 355));

        AssertRelative(
            expected: 1708.6114511913577,
            characteristicVelocity);
    }

    [Theory]
    [InlineData(0, 1.4, 287.05)]
    [InlineData(double.NaN, 1.4, 287.05)]
    [InlineData(300, 1, 287.05)]
    [InlineData(300, double.PositiveInfinity, 287.05)]
    [InlineData(300, 1.4, 0)]
    [InlineData(300, 1.4, double.NaN)]
    public void CalculateIdeal_WithInvalidGas_Throws(
        double chamberTemperature,
        double specificHeatRatio,
        double specificGasConstant)
    {
        var gas = new GasProperties(
            chamberTemperature,
            specificHeatRatio,
            specificGasConstant);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => CharacteristicVelocityCalculator.CalculateIdeal(gas));
    }

    [Fact]
    public void CalculateIdeal_WithNullGas_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => CharacteristicVelocityCalculator.CalculateIdeal(null!));
    }

    [Theory]
    [InlineData(0, 0.01, 1)]
    [InlineData(double.NaN, 0.01, 1)]
    [InlineData(1_000_000, 0, 1)]
    [InlineData(1_000_000, double.PositiveInfinity, 1)]
    [InlineData(1_000_000, 0.01, 0)]
    [InlineData(1_000_000, 0.01, double.NaN)]
    public void CalculateFromMassFlow_WithInvalidInput_Throws(
        double chamberPressure,
        double throatArea,
        double massFlowRate)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CharacteristicVelocityCalculator.CalculateFromMassFlow(
                chamberPressure,
                throatArea,
                massFlowRate));
    }

    [Fact]
    public void CalculateFromMassFlow_AvoidsIntermediateUnderflow()
    {
        var characteristicVelocity =
            CharacteristicVelocityCalculator.CalculateFromMassFlow(
                chamberPressurePascals: double.MaxValue,
                throatAreaSquareMeters: double.Epsilon,
                massFlowRateKilogramsPerSecond: double.MaxValue);

        Assert.Equal(double.Epsilon, characteristicVelocity);
    }

    [Fact]
    public void CalculateFromMassFlow_WhenResultOverflows_Throws()
    {
        Assert.Throws<OverflowException>(
            () => CharacteristicVelocityCalculator.CalculateFromMassFlow(
                chamberPressurePascals: double.MaxValue,
                throatAreaSquareMeters: double.MaxValue,
                massFlowRateKilogramsPerSecond: double.Epsilon));
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
