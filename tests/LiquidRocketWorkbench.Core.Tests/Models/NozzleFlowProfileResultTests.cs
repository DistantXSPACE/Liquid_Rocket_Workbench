using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Models;

public sealed class NozzleFlowProfileResultTests
{
    [Fact]
    public void Constructor_CopiesPointsAndProjectsStations()
    {
        var source = new List<NozzleProfilePoint>
        {
            CreatePoint(0, 0),
            CreatePoint(0.35, 1),
            CreatePoint(1, 3),
        };

        var result = new NozzleFlowProfileResult(source, throatIndex: 1);
        source.Add(CreatePoint(1, 4));

        Assert.Equal(3, result.Points.Count);
        Assert.Same(result.Points[0], result.Chamber);
        Assert.Same(result.Points[1], result.Throat);
        Assert.Same(result.Points[2], result.Exit);
    }

    [Fact]
    public void Constructor_RejectsMissingOrEmptyPoints()
    {
        Assert.Throws<ArgumentNullException>(
            () => new NozzleFlowProfileResult(null!, throatIndex: 0));
        Assert.Throws<ArgumentException>(
            () => new NozzleFlowProfileResult([], throatIndex: 0));
        Assert.Throws<ArgumentException>(
            () => new NozzleFlowProfileResult(
                [null!, CreatePoint(1, 3)],
                throatIndex: 1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void Constructor_RejectsInvalidThroatIndex(int throatIndex)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new NozzleFlowProfileResult(
                [CreatePoint(0, 0), CreatePoint(1, 3)],
                throatIndex));
    }

    private static NozzleProfilePoint CreatePoint(
        double normalizedAxialPosition,
        double machNumber)
    {
        return new NozzleProfilePoint(
            normalizedAxialPosition,
            machNumber,
            PressurePascals: 1_000_000,
            TemperatureKelvin: 300);
    }
}
