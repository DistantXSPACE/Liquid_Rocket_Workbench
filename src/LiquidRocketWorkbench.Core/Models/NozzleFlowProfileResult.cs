namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Immutable normalized chamber, throat, and exit flow-profile samples.
/// </summary>
public sealed class NozzleFlowProfileResult
{
    private readonly IReadOnlyList<NozzleProfilePoint> _points;

    public NozzleFlowProfileResult(
        IEnumerable<NozzleProfilePoint> points,
        int throatIndex)
    {
        ArgumentNullException.ThrowIfNull(points);

        var copiedPoints = points.ToArray();
        if (copiedPoints.Length == 0)
        {
            throw new ArgumentException(
                "A nozzle flow profile requires at least one point.",
                nameof(points));
        }

        if (copiedPoints.Any(static point => point is null))
        {
            throw new ArgumentException(
                "Nozzle flow profile points cannot contain null entries.",
                nameof(points));
        }

        if (throatIndex < 0 || throatIndex >= copiedPoints.Length)
        {
            throw new ArgumentOutOfRangeException(
                nameof(throatIndex),
                "The throat index must identify a profile point.");
        }

        _points = Array.AsReadOnly(copiedPoints);
        ThroatIndex = throatIndex;
    }

    public IReadOnlyList<NozzleProfilePoint> Points => _points;

    public int ThroatIndex { get; }

    public NozzleProfilePoint Chamber => Points[0];

    public NozzleProfilePoint Throat => Points[ThroatIndex];

    public NozzleProfilePoint Exit => Points[^1];
}
