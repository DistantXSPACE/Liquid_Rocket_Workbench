using LiquidRocketWorkbench.Core.Diagnostics;

namespace LiquidRocketWorkbench.Core.Models;

/// <summary>
/// Complete successful ideal-engine calculation and its non-fatal diagnostics.
/// </summary>
public sealed class EnginePerformanceResult
{
    private readonly IReadOnlyList<ValidationIssue> _diagnostics;

    public EnginePerformanceResult(
        NozzleGeometryResult geometry,
        MassFlowResult massFlow,
        NozzleExitResult nozzleExit,
        NozzleFlowProfileResult nozzleFlowProfile,
        StandardAtmosphereThrustProfileResult thrustAltitudeProfile,
        AmbientPerformanceResult selectedAmbientPerformance,
        AmbientPerformanceResult vacuumPerformance,
        AmbientPerformanceResult seaLevelPerformance,
        double characteristicVelocityMetersPerSecond,
        IEnumerable<ValidationIssue>? diagnostics = null)
    {
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(massFlow);
        ArgumentNullException.ThrowIfNull(nozzleExit);
        ArgumentNullException.ThrowIfNull(nozzleFlowProfile);
        ArgumentNullException.ThrowIfNull(thrustAltitudeProfile);
        ArgumentNullException.ThrowIfNull(selectedAmbientPerformance);
        ArgumentNullException.ThrowIfNull(vacuumPerformance);
        ArgumentNullException.ThrowIfNull(seaLevelPerformance);

        Geometry = geometry;
        MassFlow = massFlow;
        NozzleExit = nozzleExit;
        NozzleFlowProfile = nozzleFlowProfile;
        ThrustAltitudeProfile = thrustAltitudeProfile;
        SelectedAmbientPerformance = selectedAmbientPerformance;
        VacuumPerformance = vacuumPerformance;
        SeaLevelPerformance = seaLevelPerformance;
        CharacteristicVelocityMetersPerSecond =
            characteristicVelocityMetersPerSecond;
        _diagnostics = Array.AsReadOnly(
            diagnostics?.ToArray() ?? Array.Empty<ValidationIssue>());
    }

    public NozzleGeometryResult Geometry { get; }

    public MassFlowResult MassFlow { get; }

    public NozzleExitResult NozzleExit { get; }

    public NozzleFlowProfileResult NozzleFlowProfile { get; }

    public StandardAtmosphereThrustProfileResult ThrustAltitudeProfile
    {
        get;
    }

    public AmbientPerformanceResult SelectedAmbientPerformance { get; }

    public AmbientPerformanceResult VacuumPerformance { get; }

    public AmbientPerformanceResult SeaLevelPerformance { get; }

    public double CharacteristicVelocityMetersPerSecond { get; }

    public IReadOnlyList<ValidationIssue> Diagnostics => _diagnostics;
}
