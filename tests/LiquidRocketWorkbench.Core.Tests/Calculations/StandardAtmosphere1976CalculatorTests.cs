using LiquidRocketWorkbench.Core.Calculations;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class StandardAtmosphere1976CalculatorTests
{
    [Theory]
    [InlineData(0, 101_325)]
    [InlineData(10_000, 26_436.2675938076)]
    [InlineData(11_000, 22_632.0639734629)]
    [InlineData(20_000, 5_474.88866967778)]
    [InlineData(30_000, 1_171.86650015665)]
    [InlineData(32_000, 868.018684755228)]
    [InlineData(40_000, 277.521554012952)]
    [InlineData(47_000, 110.906305554966)]
    [InlineData(50_000, 75.9447675845625)]
    public void CalculatePressure_WithDocumentedCheckpoints_MatchesReference(
        double altitudeMeters,
        double expectedPressurePascals)
    {
        var pressure =
            StandardAtmosphere1976Calculator.CalculatePressurePascals(
                altitudeMeters);

        AssertRelative(expectedPressurePascals, pressure);
    }

    [Fact]
    public void CalculatePressure_IsPositiveAndStrictlyDecreasing()
    {
        var pressures = Enumerable.Range(0, 51)
            .Select(
                static index =>
                    StandardAtmosphere1976Calculator
                        .CalculatePressurePascals(index * 1000))
            .ToArray();

        Assert.All(pressures, static pressure => Assert.True(pressure > 0));
        for (var index = 1; index < pressures.Length; index++)
        {
            Assert.True(pressures[index] < pressures[index - 1]);
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(50_001)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void CalculatePressure_WithUnsupportedAltitude_Throws(
        double altitudeMeters)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => StandardAtmosphere1976Calculator
                .CalculatePressurePascals(altitudeMeters));
    }

    [Theory]
    [InlineData(101_325, 0)]
    [InlineData(26_436.2675938076, 10_000)]
    [InlineData(5_474.88866967778, 20_000)]
    [InlineData(75.9447675845625, 50_000)]
    public void TryFindAltitude_WithSupportedPressure_ReturnsAltitude(
        double pressurePascals,
        double expectedAltitudeMeters)
    {
        var found =
            StandardAtmosphere1976Calculator
                .TryFindGeopotentialAltitudeMeters(
                    pressurePascals,
                    out var altitudeMeters);

        Assert.True(found);
        Assert.Equal(expectedAltitudeMeters, altitudeMeters, precision: 5);
    }

    [Theory]
    [InlineData(101_326)]
    [InlineData(75)]
    [InlineData(0)]
    public void TryFindAltitude_WithPressureOutsideRange_ReturnsFalse(
        double pressurePascals)
    {
        var found =
            StandardAtmosphere1976Calculator
                .TryFindGeopotentialAltitudeMeters(
                    pressurePascals,
                    out var altitudeMeters);

        Assert.False(found);
        Assert.True(double.IsNaN(altitudeMeters));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void TryFindAltitude_WithInvalidPressure_Throws(
        double pressurePascals)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => StandardAtmosphere1976Calculator
                .TryFindGeopotentialAltitudeMeters(
                    pressurePascals,
                    out _));
    }

    private static void AssertRelative(
        double expected,
        double actual,
        double tolerance = 1e-10)
    {
        var relativeError = Math.Abs(actual - expected) / Math.Abs(expected);

        Assert.True(
            relativeError <= tolerance,
            $"Expected {expected:R}, actual {actual:R}, "
                + $"relative error {relativeError:R}.");
    }
}
