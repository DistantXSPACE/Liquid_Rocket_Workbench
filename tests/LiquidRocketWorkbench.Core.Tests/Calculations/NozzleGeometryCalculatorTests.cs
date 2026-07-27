using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class NozzleGeometryCalculatorTests
{
    [Fact]
    public void Calculate_WithVc01Geometry_MatchesFixedReferenceValues()
    {
        var geometry = NozzleGeometryCalculator.Calculate(
            new NozzleGeometry(
                ThroatDiameterMeters: 0.05,
                ExitDiameterMeters: 0.31622776601683794));

        AssertRelative(
            expected: 0.001963495408493621,
            geometry.ThroatAreaSquareMeters);
        AssertRelative(
            expected: 0.07853981633974483,
            geometry.ExitAreaSquareMeters);
        AssertRelative(expected: 40, geometry.ExpansionRatio);
    }

    [Fact]
    public void Calculate_WithEqualDiameters_ReturnsUnitExpansionRatio()
    {
        var geometry = NozzleGeometryCalculator.Calculate(
            new NozzleGeometry(
                ThroatDiameterMeters: 0.05,
                ExitDiameterMeters: 0.05));

        Assert.Equal(1, geometry.ExpansionRatio);
        Assert.Equal(
            geometry.ThroatAreaSquareMeters,
            geometry.ExitAreaSquareMeters);
    }

    [Theory]
    [InlineData(0, 0.1)]
    [InlineData(-0.1, 0.1)]
    [InlineData(double.NaN, 0.1)]
    [InlineData(0.1, double.PositiveInfinity)]
    public void Calculate_WithInvalidDiameter_Throws(
        double throatDiameter,
        double exitDiameter)
    {
        var nozzle = new NozzleGeometry(throatDiameter, exitDiameter);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => NozzleGeometryCalculator.Calculate(nozzle));
    }

    [Fact]
    public void Calculate_WithExitSmallerThanThroat_Throws()
    {
        var nozzle = new NozzleGeometry(
            ThroatDiameterMeters: 0.05,
            ExitDiameterMeters: 0.04);

        Assert.Throws<ArgumentException>(
            () => NozzleGeometryCalculator.Calculate(nozzle));
    }

    [Fact]
    public void Calculate_WhenAreaOverflows_Throws()
    {
        var nozzle = new NozzleGeometry(
            ThroatDiameterMeters: double.MaxValue,
            ExitDiameterMeters: double.MaxValue);

        Assert.Throws<OverflowException>(
            () => NozzleGeometryCalculator.Calculate(nozzle));
    }

    [Fact]
    public void Calculate_WithLargeRepresentableArea_RemainsFinite()
    {
        var geometry = NozzleGeometryCalculator.Calculate(
            new NozzleGeometry(
                ThroatDiameterMeters: 1e154,
                ExitDiameterMeters: 1e154));

        Assert.True(double.IsFinite(geometry.ThroatAreaSquareMeters));
        Assert.True(double.IsFinite(geometry.ExitAreaSquareMeters));
        Assert.Equal(1, geometry.ExpansionRatio);
    }

    [Fact]
    public void Calculate_WhenAreaUnderflows_Throws()
    {
        var nozzle = new NozzleGeometry(
            ThroatDiameterMeters: double.Epsilon,
            ExitDiameterMeters: double.Epsilon);

        Assert.Throws<ArithmeticException>(
            () => NozzleGeometryCalculator.Calculate(nozzle));
    }

    [Fact]
    public void Calculate_WithNullGeometry_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => NozzleGeometryCalculator.Calculate(null!));
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
