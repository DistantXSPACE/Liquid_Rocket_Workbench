using LiquidRocketWorkbench.Core.Calculations;
using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Calculations;

public sealed class StandardAtmosphereThrustProfileCalculatorTests
{
    [Fact]
    public void Calculate_WithVc04State_MatchesFixedAltitudeSweep()
    {
        var result = Calculate(
            selectedAmbientPressurePascals:
                PhysicalConstants.StandardSeaLevelPressurePascals);

        Assert.Equal(
            StandardAtmosphereThrustProfileCalculator.ProfilePointCount,
            result.Points.Count);
        AssertPoint(
            result.Points[0],
            expectedAltitudeMeters: 0,
            expectedPressurePascals: 101_325,
            expectedThrustNewtons: 21_314.7581691926);
        AssertPoint(
            result.Points[10],
            expectedAltitudeMeters: 10_000,
            expectedPressurePascals: 26_436.2675938076,
            expectedThrustNewtons: 27_196.5054582912);
        AssertPoint(
            result.Points[20],
            expectedAltitudeMeters: 20_000,
            expectedPressurePascals: 5_474.88866967778,
            expectedThrustNewtons: 28_842.8083092202);
        AssertPoint(
            result.Points[30],
            expectedAltitudeMeters: 30_000,
            expectedPressurePascals: 1_171.86650015665,
            expectedThrustNewtons: 29_180.7668801202);
        AssertPoint(
            result.Points[40],
            expectedAltitudeMeters: 40_000,
            expectedPressurePascals: 277.521554012952,
            expectedThrustNewtons: 29_251.0085679347);
        AssertPoint(
            result.Points[50],
            expectedAltitudeMeters: 50_000,
            expectedPressurePascals: 75.9447675845625,
            expectedThrustNewtons: 29_266.8403717192);
        Assert.Equal(0, result.SelectedAmbientEquivalentGeopotentialAltitudeMeters);
    }

    [Fact]
    public void Calculate_ReturnsMonotonicPressureAndThrust()
    {
        var result = Calculate(
            selectedAmbientPressurePascals:
                PhysicalConstants.StandardSeaLevelPressurePascals);

        for (var index = 1; index < result.Points.Count; index++)
        {
            Assert.True(
                result.Points[index].AmbientPressurePascals
                < result.Points[index - 1].AmbientPressurePascals);
            Assert.True(
                result.Points[index].TotalThrustNewtons
                > result.Points[index - 1].TotalThrustNewtons);
        }

        Assert.Equal(
            NozzleExpansionState.Overexpanded,
            result.SeaLevel.NozzleExpansionState);
        Assert.Equal(
            NozzleExpansionState.Underexpanded,
            result.MaximumAltitude.NozzleExpansionState);
    }

    [Fact]
    public void Calculate_WithSupportedSelectedPressure_ProvidesEquivalentAltitude()
    {
        var result = Calculate(
            selectedAmbientPressurePascals: 26_436.2675938076);

        Assert.NotNull(
            result.SelectedAmbientEquivalentGeopotentialAltitudeMeters);
        Assert.Equal(
            10_000,
            result.SelectedAmbientEquivalentGeopotentialAltitudeMeters.Value,
            precision: 5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(200_000)]
    public void Calculate_WithSelectedPressureOutsideSweep_HasNoMarker(
        double selectedAmbientPressurePascals)
    {
        var result = Calculate(selectedAmbientPressurePascals);

        Assert.Null(
            result.SelectedAmbientEquivalentGeopotentialAltitudeMeters);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Calculate_WithInvalidSelectedPressure_Throws(
        double selectedAmbientPressurePascals)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Calculate(selectedAmbientPressurePascals));
    }

    [Fact]
    public void Calculate_WithNullGeometryOrExit_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => StandardAtmosphereThrustProfileCalculator.Calculate(
                selectedAmbientPressurePascals: 101_325,
                chamberPressurePascals: 8_000_000,
                massFlowRateKilogramsPerSecond: 9.193408634242926,
                geometry: null!,
                CreateNozzleExit()));
        Assert.Throws<ArgumentNullException>(
            () => StandardAtmosphereThrustProfileCalculator.Calculate(
                selectedAmbientPressurePascals: 101_325,
                chamberPressurePascals: 8_000_000,
                massFlowRateKilogramsPerSecond: 9.193408634242926,
                CreateGeometry(),
                nozzleExit: null!));
    }

    private static StandardAtmosphereThrustProfileResult Calculate(
        double selectedAmbientPressurePascals)
    {
        return StandardAtmosphereThrustProfileCalculator.Calculate(
            selectedAmbientPressurePascals,
            chamberPressurePascals: 8_000_000,
            massFlowRateKilogramsPerSecond: 9.193408634242926,
            CreateGeometry(),
            CreateNozzleExit());
    }

    private static NozzleGeometryResult CreateGeometry()
    {
        return new NozzleGeometryResult(
            ThroatAreaSquareMeters: 0.001963495408493621,
            ExitAreaSquareMeters: 0.07853981633974483,
            ExpansionRatio: 40);
    }

    private static NozzleExitResult CreateNozzleExit()
    {
        return new NozzleExitResult(
            MachNumber: 4.355537019605814,
            PressurePascals: 15_436.918385488754,
            TemperatureKelvin: 1133.8686466837182,
            VelocityMetersPerSecond: 3052.229422333306);
    }

    private static void AssertPoint(
        StandardAtmosphereThrustPoint point,
        double expectedAltitudeMeters,
        double expectedPressurePascals,
        double expectedThrustNewtons)
    {
        Assert.Equal(
            expectedAltitudeMeters,
            point.GeopotentialAltitudeMeters);
        AssertRelative(
            expectedPressurePascals,
            point.AmbientPressurePascals);
        AssertRelative(
            expectedThrustNewtons,
            point.TotalThrustNewtons);
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
