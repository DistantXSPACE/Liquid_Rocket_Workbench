using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Models;

public sealed class StandardAtmosphereThrustProfileResultTests
{
    [Fact]
    public void Constructor_CopiesPointsAndProjectsEndpoints()
    {
        var source = new List<StandardAtmosphereThrustPoint>
        {
            CreatePoint(0),
            CreatePoint(50_000),
        };

        var result = new StandardAtmosphereThrustProfileResult(
            source,
            selectedAmbientEquivalentGeopotentialAltitudeMeters: 10_000);
        source.Add(CreatePoint(60_000));

        Assert.Equal(2, result.Points.Count);
        Assert.Same(result.Points[0], result.SeaLevel);
        Assert.Same(result.Points[1], result.MaximumAltitude);
        Assert.Equal(
            10_000,
            result.SelectedAmbientEquivalentGeopotentialAltitudeMeters);
    }

    [Fact]
    public void Constructor_RejectsMissingEmptyOrNullPoints()
    {
        Assert.Throws<ArgumentNullException>(
            () => new StandardAtmosphereThrustProfileResult(
                null!,
                selectedAmbientEquivalentGeopotentialAltitudeMeters: null));
        Assert.Throws<ArgumentException>(
            () => new StandardAtmosphereThrustProfileResult(
                [],
                selectedAmbientEquivalentGeopotentialAltitudeMeters: null));
        Assert.Throws<ArgumentException>(
            () => new StandardAtmosphereThrustProfileResult(
                [null!, CreatePoint(50_000)],
                selectedAmbientEquivalentGeopotentialAltitudeMeters: null));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Constructor_RejectsInvalidEquivalentAltitude(
        double equivalentAltitudeMeters)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new StandardAtmosphereThrustProfileResult(
                [CreatePoint(0)],
                equivalentAltitudeMeters));
    }

    private static StandardAtmosphereThrustPoint CreatePoint(
        double altitudeMeters)
    {
        return new StandardAtmosphereThrustPoint(
            altitudeMeters,
            AmbientPressurePascals: 101_325,
            TotalThrustNewtons: 20_000,
            NozzleExpansionState.Overexpanded);
    }
}
