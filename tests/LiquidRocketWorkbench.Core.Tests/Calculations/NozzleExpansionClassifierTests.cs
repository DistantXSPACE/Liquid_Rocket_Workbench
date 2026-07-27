using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class NozzleExpansionClassifierTests
{
    [Theory]
    [InlineData(100_000, 101_900, NozzleExpansionState.IdeallyExpanded)]
    [InlineData(100_000, 102_100, NozzleExpansionState.Overexpanded)]
    [InlineData(100_000, 97_000, NozzleExpansionState.Underexpanded)]
    public void Classify_WithVc05Cases_ReturnsExpectedState(
        double exitPressure,
        double ambientPressure,
        NozzleExpansionState expected)
    {
        var actual = NozzleExpansionClassifier.Classify(
            exitPressure,
            ambientPressure);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(100, 98)]
    [InlineData(98, 100)]
    public void Classify_AtTwoPercentBoundary_ReturnsIdeallyExpanded(
        double exitPressure,
        double ambientPressure)
    {
        var actual = NozzleExpansionClassifier.Classify(
            exitPressure,
            ambientPressure);

        Assert.Equal(NozzleExpansionState.IdeallyExpanded, actual);
    }

    [Theory]
    [InlineData(100, 97.999, NozzleExpansionState.Underexpanded)]
    [InlineData(97.999, 100, NozzleExpansionState.Overexpanded)]
    public void Classify_JustOutsideBoundary_ReturnsExpansionDirection(
        double exitPressure,
        double ambientPressure,
        NozzleExpansionState expected)
    {
        var actual = NozzleExpansionClassifier.Classify(
            exitPressure,
            ambientPressure);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100_000, 100_000)]
    public void Classify_WithEqualPressures_ReturnsIdeallyExpanded(
        double exitPressure,
        double ambientPressure)
    {
        var actual = NozzleExpansionClassifier.Classify(
            exitPressure,
            ambientPressure);

        Assert.Equal(NozzleExpansionState.IdeallyExpanded, actual);
    }

    [Fact]
    public void Classify_WithExtremeFinitePressures_RemainsDeterministic()
    {
        Assert.Equal(
            NozzleExpansionState.Underexpanded,
            NozzleExpansionClassifier.Classify(double.MaxValue, 0));
        Assert.Equal(
            NozzleExpansionState.Overexpanded,
            NozzleExpansionClassifier.Classify(0, double.MaxValue));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(double.NaN, 0)]
    [InlineData(double.PositiveInfinity, 0)]
    [InlineData(0, -1)]
    [InlineData(0, double.NaN)]
    [InlineData(0, double.PositiveInfinity)]
    public void Classify_WithInvalidPressure_Throws(
        double exitPressure,
        double ambientPressure)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => NozzleExpansionClassifier.Classify(
                exitPressure,
                ambientPressure));
    }
}
