using LiquidRocketWorkbench.App.ViewModels;
using LiquidRocketWorkbench.Core;
using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.App.Tests.ViewModels;

public sealed class ThrustAltitudeViewModelTests
{
    [Fact]
    public void Constructor_WithNullPerformance_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new ThrustAltitudeViewModel(null!));
    }

    [Fact]
    public void Constructor_MapsCompleteCurveToResponsiveCanvas()
    {
        var viewModel = new ThrustAltitudeViewModel(CreatePerformance());

        Assert.Equal(
            StandardAtmosphereThrustProfileCalculator.ProfilePointCount,
            viewModel.CurvePoints.Count);
        Assert.Equal(80, viewModel.CurvePoints[0].X);
        Assert.Equal(188, viewModel.CurvePoints[0].Y);
        Assert.Equal(865, viewModel.CurvePoints[^1].X);
        Assert.True(viewModel.CurvePoints[^1].Y > 22);
        Assert.True(viewModel.CurvePoints[^1].Y < 23);

        for (var index = 1; index < viewModel.CurvePoints.Count; index++)
        {
            Assert.True(
                viewModel.CurvePoints[index].X
                > viewModel.CurvePoints[index - 1].X);
            Assert.True(
                viewModel.CurvePoints[index].Y
                < viewModel.CurvePoints[index - 1].Y);
        }
    }

    [Fact]
    public void Constructor_ProjectsPairedAltitudePressureAndThrustStations()
    {
        var viewModel = new ThrustAltitudeViewModel(CreatePerformance());

        Assert.Equal(
            ["0 km", "10 km", "20 km", "30 km", "40 km", "50 km"],
            viewModel.Stations.Select(static station => station.AltitudeText));
        Assert.Equal("101.325 kPa", viewModel.Stations[0].AmbientPressureText);
        Assert.Equal("21.315 kN", viewModel.Stations[0].ThrustText);
        Assert.Equal("26.436 kPa", viewModel.Stations[1].AmbientPressureText);
        Assert.Equal("27.197 kN", viewModel.Stations[1].ThrustText);
        Assert.Equal("0.076 kPa", viewModel.Stations[^1].AmbientPressureText);
        Assert.Equal("29.267 kN", viewModel.Stations[^1].ThrustText);
        Assert.Contains("vacuum reference", viewModel.CurveAccessibilitySummary);
    }

    [Fact]
    public void Constructor_WithSeaLevelSelection_ProjectsSelectedMarker()
    {
        var viewModel = new ThrustAltitudeViewModel(CreatePerformance());

        Assert.True(viewModel.HasSelectedStandardAltitude);
        Assert.Equal(74, viewModel.SelectedMarkerLeft);
        Assert.Equal(182, viewModel.SelectedMarkerTop);
        Assert.Contains("101.325 kPa", viewModel.SelectedAmbientSummary);
        Assert.Contains("standard equivalent 0.0 km", viewModel.SelectedAmbientSummary);
    }

    [Fact]
    public void Constructor_WithVacuumSelection_OmitsStandardAltitudeMarker()
    {
        var viewModel = new ThrustAltitudeViewModel(
            CreatePerformance(ambientPressurePascals: 0));

        Assert.False(viewModel.HasSelectedStandardAltitude);
        Assert.Contains(
            "outside the 0–50 km standard-altitude range",
            viewModel.SelectedAmbientSummary);
    }

    [Fact]
    public void Constructor_CommunicatesAtmosphereAndModelLimits()
    {
        var viewModel = new ThrustAltitudeViewModel(CreatePerformance());

        Assert.Contains(
            "geopotential",
            viewModel.ModelNotice,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not live weather", viewModel.ModelNotice);
        Assert.Contains("exit state is held fixed", viewModel.ModelNotice);
        Assert.Contains("separation", viewModel.ModelNotice);
    }

    private static EnginePerformanceResult CreatePerformance(
        double ambientPressurePascals =
            PhysicalConstants.StandardSeaLevelPressurePascals)
    {
        var inputs = new EngineInputs(
            PropellantLabel: "VC-04 synthetic constant-property gas",
            ChamberPressurePascals: 8_000_000,
            MixtureRatio: 3.5,
            Nozzle: new NozzleGeometry(
                ThroatDiameterMeters: 0.05,
                ExitDiameterMeters: 0.31622776601683794),
            Gas: new GasProperties(
                ChamberTemperatureKelvin: 3500,
                SpecificHeatRatio: 1.22,
                SpecificGasConstantJoulesPerKilogramKelvin: 355),
            AmbientPressurePascals: ambientPressurePascals);
        var outcome = new EnginePerformanceCalculator().Calculate(inputs);

        Assert.True(outcome.IsSuccess);
        return Assert.IsType<EnginePerformanceResult>(outcome.Performance);
    }
}
