using System.Globalization;
using LiquidRocketWorkbench.App.ViewModels;
using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.App.Tests.ViewModels;

public sealed class NozzleFlowProfileViewModelTests
{
    [Fact]
    public void Constructor_WithNullProfile_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new NozzleFlowProfileViewModel(null!));
    }

    [Fact]
    public void Constructor_ProjectsThreeSeriesInWorkflowOrder()
    {
        var viewModel = new NozzleFlowProfileViewModel(CreateProfile());

        Assert.Equal(
            ["Static pressure", "Static temperature", "Mach number"],
            viewModel.Series.Select(static series => series.Title));
        Assert.Same(viewModel.Pressure, viewModel.Series[0]);
        Assert.Same(viewModel.Temperature, viewModel.Series[1]);
        Assert.Same(viewModel.Mach, viewModel.Series[2]);
        Assert.Contains("not a solved hardware contour", viewModel.ModelNotice);
        Assert.Contains("throat 0.35", viewModel.AxisCaption);
    }

    [Fact]
    public void Constructor_MapsProfileToResponsiveFixedCanvas()
    {
        var viewModel = new NozzleFlowProfileViewModel(CreateProfile());

        Assert.Equal(
            NozzleFlowProfileCalculator.ProfilePointCount,
            viewModel.Pressure.PlotPoints.Count);
        Assert.Equal(80, viewModel.Pressure.PlotPoints[0].X);
        Assert.Equal(18, viewModel.Pressure.PlotPoints[0].Y);
        Assert.Equal(
            354.75,
            viewModel.Pressure.PlotPoints[
                NozzleFlowProfileCalculator.ChamberToThroatSegmentCount].X,
            10);
        Assert.Equal(865, viewModel.Pressure.PlotPoints[^1].X);
        Assert.Equal(108, viewModel.Pressure.PlotPoints[^1].Y);

        Assert.Equal(108, viewModel.Mach.PlotPoints[0].Y);
        Assert.Equal(18, viewModel.Mach.PlotPoints[^1].Y);
    }

    [Fact]
    public void Constructor_FormatsAccessibleStationValues()
    {
        var profile = CreateProfile();
        var viewModel = new NozzleFlowProfileViewModel(profile);

        Assert.Equal(
            ["Chamber", "Throat", "Exit"],
            viewModel.Pressure.Stations.Select(static station => station.Name));
        Assert.Equal("0.00", Normalize(viewModel.Pressure.Stations[0].PositionText));
        Assert.Equal("0.35", Normalize(viewModel.Pressure.Stations[1].PositionText));
        Assert.Equal("1.00", Normalize(viewModel.Pressure.Stations[2].PositionText));
        Assert.Contains(
            (profile.Exit.PressurePascals / 1000).ToString(
                "N1",
                CultureInfo.CurrentCulture),
            viewModel.Pressure.Stations[2].ValueText);
        Assert.Contains(
            profile.Exit.MachNumber.ToString(
                "N3",
                CultureInfo.CurrentCulture),
            viewModel.Mach.AccessibilitySummary);
        Assert.Contains("Exit", viewModel.Temperature.AccessibilitySummary);
    }

    [Fact]
    public void Series_WithConstantValues_Throws()
    {
        var profile = CreateProfile();

        Assert.Throws<ArgumentException>(
            () => new NozzleProfileSeriesViewModel(
                "Constant",
                "unit",
                "#000000",
                profile,
                static _ => 1,
                "N1"));
    }

    private static NozzleFlowProfileResult CreateProfile()
    {
        return NozzleFlowProfileCalculator.Calculate(
            exitMachNumber: 4.355537019605814,
            chamberPressurePascals: 8_000_000,
            new GasProperties(
                ChamberTemperatureKelvin: 3500,
                SpecificHeatRatio: 1.22,
                SpecificGasConstantJoulesPerKilogramKelvin: 355));
    }

    private static string Normalize(string value)
    {
        return value.Replace(',', '.');
    }
}
