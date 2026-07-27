using LiquidRocketWorkbench.Core.Calculations;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class PropellantFlowCalculatorTests
{
    [Fact]
    public void Split_WithVc01Flow_MatchesFixedReferenceValues()
    {
        var split = PropellantFlowCalculator.Split(
            totalMassFlowRateKilogramsPerSecond: 20,
            mixtureRatio: 3.5);

        AssertRelative(
            expected: 15.555555555555555,
            split.OxidizerMassFlowRateKilogramsPerSecond);
        AssertRelative(
            expected: 4.444444444444445,
            split.FuelMassFlowRateKilogramsPerSecond);
        Assert.Equal(20, split.TotalMassFlowRateKilogramsPerSecond);
    }

    [Fact]
    public void Split_WithUnitMixtureRatio_ReturnsEqualFlows()
    {
        var split = PropellantFlowCalculator.Split(
            totalMassFlowRateKilogramsPerSecond: 20,
            mixtureRatio: 1);

        Assert.Equal(10, split.OxidizerMassFlowRateKilogramsPerSecond);
        Assert.Equal(10, split.FuelMassFlowRateKilogramsPerSecond);
        Assert.Equal(20, split.TotalMassFlowRateKilogramsPerSecond);
    }

    [Fact]
    public void Split_WithVerySmallMixtureRatio_PreservesBothComponents()
    {
        var split = PropellantFlowCalculator.Split(
            totalMassFlowRateKilogramsPerSecond: 20,
            mixtureRatio: 1e-200);

        Assert.True(split.OxidizerMassFlowRateKilogramsPerSecond > 0);
        Assert.True(split.FuelMassFlowRateKilogramsPerSecond > 0);
        Assert.Equal(20, split.TotalMassFlowRateKilogramsPerSecond);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(double.NaN, 1)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, double.PositiveInfinity)]
    public void Split_WithInvalidInput_Throws(
        double totalMassFlowRate,
        double mixtureRatio)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PropellantFlowCalculator.Split(
                totalMassFlowRate,
                mixtureRatio));
    }

    [Fact]
    public void Split_WhenAComponentUnderflows_Throws()
    {
        Assert.Throws<ArithmeticException>(
            () => PropellantFlowCalculator.Split(
                totalMassFlowRateKilogramsPerSecond: double.Epsilon,
                mixtureRatio: double.Epsilon));
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
