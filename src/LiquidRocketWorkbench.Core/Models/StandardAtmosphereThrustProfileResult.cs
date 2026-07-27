namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Immutable standard-atmosphere thrust sweep and selected-pressure context.
/// </summary>
public sealed class StandardAtmosphereThrustProfileResult
{
    private readonly IReadOnlyList<StandardAtmosphereThrustPoint> _points;

    public StandardAtmosphereThrustProfileResult(
        IEnumerable<StandardAtmosphereThrustPoint> points,
        double? selectedAmbientEquivalentGeopotentialAltitudeMeters)
    {
        ArgumentNullException.ThrowIfNull(points);

        var copiedPoints = points.ToArray();
        if (copiedPoints.Length == 0)
        {
            throw new ArgumentException(
                "A thrust-altitude profile requires at least one point.",
                nameof(points));
        }

        if (copiedPoints.Any(static point => point is null))
        {
            throw new ArgumentException(
                "Thrust-altitude points cannot contain null entries.",
                nameof(points));
        }

        if (selectedAmbientEquivalentGeopotentialAltitudeMeters
                is double selectedAltitude
            && (!double.IsFinite(selectedAltitude) || selectedAltitude < 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectedAmbientEquivalentGeopotentialAltitudeMeters),
                selectedAltitude,
                "Equivalent standard altitude must be finite and nonnegative.");
        }

        _points = Array.AsReadOnly(copiedPoints);
        SelectedAmbientEquivalentGeopotentialAltitudeMeters =
            selectedAmbientEquivalentGeopotentialAltitudeMeters;
    }

    public IReadOnlyList<StandardAtmosphereThrustPoint> Points => _points;

    public StandardAtmosphereThrustPoint SeaLevel => Points[0];

    public StandardAtmosphereThrustPoint MaximumAltitude => Points[^1];

    public double? SelectedAmbientEquivalentGeopotentialAltitudeMeters
    {
        get;
    }
}
