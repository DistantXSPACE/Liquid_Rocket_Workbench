using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class NozzleFlowProfileCalculatorTests
{
    private const double Vc04ExitMachNumber = 4.355537019605814;

    [Fact]
    public void Calculate_WithVc04Inputs_MatchesFixedStationStates()
    {
        var result = NozzleFlowProfileCalculator.Calculate(
            Vc04ExitMachNumber,
            chamberPressurePascals: 8_000_000,
            CreateVc04Gas());

        Assert.Equal(
            NozzleFlowProfileCalculator.ProfilePointCount,
            result.Points.Count);
        Assert.Equal(
            NozzleFlowProfileCalculator.ChamberToThroatSegmentCount,
            result.ThroatIndex);

        Assert.Equal(0, result.Chamber.NormalizedAxialPosition);
        Assert.Equal(0, result.Chamber.MachNumber);
        Assert.Equal(8_000_000, result.Chamber.PressurePascals);
        Assert.Equal(3500, result.Chamber.TemperatureKelvin);

        Assert.Equal(
            NozzleFlowProfileCalculator.NormalizedThroatPosition,
            result.Throat.NormalizedAxialPosition);
        Assert.Equal(1, result.Throat.MachNumber);
        AssertRelative(
            expected: 4_484_907.27280117,
            result.Throat.PressurePascals);
        AssertRelative(
            expected: 3153.15315315315,
            result.Throat.TemperatureKelvin);

        Assert.Equal(1, result.Exit.NormalizedAxialPosition);
        Assert.Equal(Vc04ExitMachNumber, result.Exit.MachNumber);
        AssertRelative(
            expected: 15_436.918385488754,
            result.Exit.PressurePascals);
        AssertRelative(
            expected: 1133.8686466837182,
            result.Exit.TemperatureKelvin);
    }

    [Fact]
    public void Calculate_UsesDocumentedSmoothNormalizedInterpolation()
    {
        var result = NozzleFlowProfileCalculator.Calculate(
            Vc04ExitMachNumber,
            chamberPressurePascals: 8_000_000,
            CreateVc04Gas());

        var convergingMidpoint = result.Points[4];
        Assert.Equal(0.175, convergingMidpoint.NormalizedAxialPosition, 12);
        Assert.Equal(0.5, convergingMidpoint.MachNumber, 12);

        var divergingMidpoint = result.Points[16];
        Assert.Equal(0.675, divergingMidpoint.NormalizedAxialPosition, 12);
        Assert.Equal(
            2.677768509802907,
            divergingMidpoint.MachNumber,
            12);
    }

    [Fact]
    public void Calculate_ReturnsFiniteMonotonicIdealProfiles()
    {
        var result = NozzleFlowProfileCalculator.Calculate(
            Vc04ExitMachNumber,
            chamberPressurePascals: 8_000_000,
            CreateVc04Gas());

        Assert.All(
            result.Points,
            static point =>
            {
                Assert.True(double.IsFinite(point.NormalizedAxialPosition));
                Assert.True(double.IsFinite(point.MachNumber));
                Assert.True(double.IsFinite(point.PressurePascals));
                Assert.True(double.IsFinite(point.TemperatureKelvin));
            });

        AssertMonotonic(
            result.Points.Select(static point => point.NormalizedAxialPosition),
            increasing: true);
        AssertMonotonic(
            result.Points.Select(static point => point.MachNumber),
            increasing: true);
        AssertMonotonic(
            result.Points.Select(static point => point.PressurePascals),
            increasing: false);
        AssertMonotonic(
            result.Points.Select(static point => point.TemperatureKelvin),
            increasing: false);
    }

    [Fact]
    public void Calculate_WithMachOneExit_RemainsChokedAfterThroat()
    {
        var result = NozzleFlowProfileCalculator.Calculate(
            exitMachNumber: 1,
            chamberPressurePascals: 1_000_000,
            new GasProperties(
                ChamberTemperatureKelvin: 300,
                SpecificHeatRatio: 1.4,
                SpecificGasConstantJoulesPerKilogramKelvin: 287.05));

        Assert.All(
            result.Points.Skip(result.ThroatIndex),
            static point => Assert.Equal(1, point.MachNumber));
    }

    [Theory]
    [InlineData(0, 1_000_000)]
    [InlineData(0.9, 1_000_000)]
    [InlineData(double.NaN, 1_000_000)]
    [InlineData(double.PositiveInfinity, 1_000_000)]
    [InlineData(2, 0)]
    [InlineData(2, double.NaN)]
    public void Calculate_WithInvalidScalarInput_Throws(
        double exitMachNumber,
        double chamberPressurePascals)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => NozzleFlowProfileCalculator.Calculate(
                exitMachNumber,
                chamberPressurePascals,
                CreateVc04Gas()));
    }

    [Theory]
    [InlineData(0, 1.22, 355)]
    [InlineData(3500, 1, 355)]
    [InlineData(3500, 1.22, 0)]
    public void Calculate_WithInvalidGas_Throws(
        double chamberTemperatureKelvin,
        double specificHeatRatio,
        double specificGasConstant)
    {
        var gas = new GasProperties(
            chamberTemperatureKelvin,
            specificHeatRatio,
            specificGasConstant);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => NozzleFlowProfileCalculator.Calculate(
                Vc04ExitMachNumber,
                chamberPressurePascals: 8_000_000,
                gas));
    }

    [Fact]
    public void Calculate_WithNullGas_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => NozzleFlowProfileCalculator.Calculate(
                Vc04ExitMachNumber,
                chamberPressurePascals: 8_000_000,
                gas: null!));
    }

    private static GasProperties CreateVc04Gas()
    {
        return new GasProperties(
            ChamberTemperatureKelvin: 3500,
            SpecificHeatRatio: 1.22,
            SpecificGasConstantJoulesPerKilogramKelvin: 355);
    }

    private static void AssertMonotonic(
        IEnumerable<double> values,
        bool increasing)
    {
        var points = values.ToArray();
        for (var index = 1; index < points.Length; index++)
        {
            if (increasing)
            {
                Assert.True(points[index] >= points[index - 1]);
            }
            else
            {
                Assert.True(points[index] <= points[index - 1]);
            }
        }
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
