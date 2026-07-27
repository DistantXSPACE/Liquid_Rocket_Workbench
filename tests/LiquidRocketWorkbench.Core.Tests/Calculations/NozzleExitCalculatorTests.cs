using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class NozzleExitCalculatorTests
{
    [Fact]
    public void Calculate_WithVc02Inputs_MatchesFixedStateRatios()
    {
        var result = NozzleExitCalculator.Calculate(
            machNumber: 2,
            chamberPressurePascals: 1,
            gas: new GasProperties(
                ChamberTemperatureKelvin: 1,
                SpecificHeatRatio: 1.4,
                SpecificGasConstantJoulesPerKilogramKelvin: 1));

        AssertRelative(
            expected: 0.5555555555555556,
            result.TemperatureKelvin);
        AssertRelative(
            expected: 0.12780452546295096,
            result.PressurePascals);
    }

    [Fact]
    public void Calculate_WithVc04Inputs_MatchesFixedExitState()
    {
        var result = NozzleExitCalculator.Calculate(
            machNumber: 4.355537019605814,
            chamberPressurePascals: 8_000_000,
            gas: new GasProperties(
                ChamberTemperatureKelvin: 3500,
                SpecificHeatRatio: 1.22,
                SpecificGasConstantJoulesPerKilogramKelvin: 355));

        Assert.Equal(4.355537019605814, result.MachNumber);
        AssertRelative(
            expected: 1133.8686466837182,
            result.TemperatureKelvin);
        AssertRelative(
            expected: 15_436.918385488754,
            result.PressurePascals);
        AssertRelative(
            expected: 3052.229422333306,
            result.VelocityMetersPerSecond);
    }

    [Theory]
    [InlineData(0, 1_000_000)]
    [InlineData(-1, 1_000_000)]
    [InlineData(double.NaN, 1_000_000)]
    [InlineData(2, 0)]
    [InlineData(2, double.PositiveInfinity)]
    public void Calculate_WithInvalidScalarInput_Throws(
        double machNumber,
        double chamberPressure)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => NozzleExitCalculator.Calculate(
                machNumber,
                chamberPressure,
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
            () => NozzleExitCalculator.Calculate(
                machNumber: 2,
                chamberPressurePascals: 1_000_000,
                gas));
    }

    [Fact]
    public void Calculate_WithNullGas_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => NozzleExitCalculator.Calculate(
                machNumber: 2,
                chamberPressurePascals: 1_000_000,
                gas: null!));
    }

    [Fact]
    public void Calculate_WhenVelocityOverflows_Throws()
    {
        var gas = new GasProperties(
            ChamberTemperatureKelvin: double.MaxValue,
            SpecificHeatRatio: 1.4,
            SpecificGasConstantJoulesPerKilogramKelvin: double.MaxValue);

        Assert.Throws<OverflowException>(
            () => NozzleExitCalculator.Calculate(
                machNumber: 2,
                chamberPressurePascals: 1,
                gas));
    }

    [Fact]
    public void Calculate_WhenStaticPressureUnderflows_Throws()
    {
        Assert.Throws<ArithmeticException>(
            () => NozzleExitCalculator.Calculate(
                machNumber: double.MaxValue,
                chamberPressurePascals: 1,
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
