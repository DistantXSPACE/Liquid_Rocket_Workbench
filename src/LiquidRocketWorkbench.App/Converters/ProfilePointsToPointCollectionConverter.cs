using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using LiquidRocketWorkbench.App.ViewModels;

namespace LiquidRocketWorkbench.App.Converters;

/// <summary>
/// Converts immutable chart points to the WPF collection consumed by Polyline.
/// </summary>
public sealed class ProfilePointsToPointCollectionConverter : IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value
            is not IEnumerable<NozzleProfileChartPointViewModel> points)
        {
            return new PointCollection();
        }

        var collection = new PointCollection(
            points.Select(static point => new Point(point.X, point.Y)));
        collection.Freeze();
        return collection;
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
