using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Tests.Models;

public sealed class EngineInputsTests
{
    [Fact]
    public void Constructor_PreservesCanonicalSiOperatingPoint()
    {
        var nozzle = new NozzleGeometry(
            ThroatDiameterMeters: 0.05,
            ExitDiameterMeters: 0.31622776601683794);
        var gas = new GasProperties(
            ChamberTemperatureKelvin: 3500,
            SpecificHeatRatio: 1.22,
            SpecificGasConstantJoulesPerKilogramKelvin: 355);

        var inputs = new EngineInputs(
            PropellantLabel: "LOX / Methane",
            ChamberPressurePascals: 8_000_000,
            MixtureRatio: 3.5,
            Nozzle: nozzle,
            Gas: gas,
            AmbientPressurePascals: 101_325,
            TargetMassFlowRateKilogramsPerSecond: 20,
            BurnDurationSeconds: 10);

        Assert.Equal("LOX / Methane", inputs.PropellantLabel);
        Assert.Equal(8_000_000, inputs.ChamberPressurePascals);
        Assert.Equal(3.5, inputs.MixtureRatio);
        Assert.Same(nozzle, inputs.Nozzle);
        Assert.Same(gas, inputs.Gas);
        Assert.Equal(101_325, inputs.AmbientPressurePascals);
        Assert.Equal(20, inputs.TargetMassFlowRateKilogramsPerSecond);
        Assert.Equal(10, inputs.BurnDurationSeconds);
    }

    [Fact]
    public void Constructor_AllowsOptionalComparisonInputsToBeOmitted()
    {
        var inputs = new EngineInputs(
            PropellantLabel: "Custom",
            ChamberPressurePascals: 1_000_000,
            MixtureRatio: 2.5,
            Nozzle: new NozzleGeometry(0.01, 0.02),
            Gas: new GasProperties(3000, 1.2, 350),
            AmbientPressurePascals: 0);

        Assert.Null(inputs.TargetMassFlowRateKilogramsPerSecond);
        Assert.Null(inputs.BurnDurationSeconds);
    }
}
