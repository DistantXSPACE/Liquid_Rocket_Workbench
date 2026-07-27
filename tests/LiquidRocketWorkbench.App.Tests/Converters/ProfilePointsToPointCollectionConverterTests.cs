using System.Globalization;
using System.Windows.Media;
using LiquidRocketWorkbench.App.Converters;
using LiquidRocketWorkbench.App.ViewModels;

namespace LiquidRocketWorkbench.App.Tests.Converters;

public sealed class ProfilePointsToPointCollectionConverterTests
{
    private readonly ProfilePointsToPointCollectionConverter _converter =
        new();

    [Fact]
    public void Convert_ProjectsAndFreezesPointCollection()
    {
        var source = new[]
        {
            new NozzleProfileChartPointViewModel(10, 20),
            new NozzleProfileChartPointViewModel(30, 40),
        };

        var converted = _converter.Convert(
            source,
            typeof(PointCollection),
            parameter: null,
            CultureInfo.InvariantCulture);

        var points = Assert.IsType<PointCollection>(converted);
        Assert.True(points.IsFrozen);
        Assert.Equal(2, points.Count);
        Assert.Equal(10, points[0].X);
        Assert.Equal(20, points[0].Y);
        Assert.Equal(30, points[1].X);
        Assert.Equal(40, points[1].Y);
    }

    [Fact]
    public void Convert_WithUnsupportedValue_ReturnsEmptyCollection()
    {
        var converted = _converter.Convert(
            "not points",
            typeof(PointCollection),
            parameter: null,
            CultureInfo.InvariantCulture);

        Assert.Empty(Assert.IsType<PointCollection>(converted));
    }

    [Fact]
    public void ConvertBack_Throws()
    {
        Assert.Throws<NotSupportedException>(
            () => _converter.ConvertBack(
                value: null,
                typeof(IEnumerable<NozzleProfileChartPointViewModel>),
                parameter: null,
                CultureInfo.InvariantCulture));
    }
}
