using LiquidRocketWorkbench.App.ViewModels;

namespace LiquidRocketWorkbench.App.Tests.ViewModels;

public sealed class ThermodynamicPresetCatalogTests
{
    [Fact]
    public void BuiltIn_HasUniqueStableIdsAndOneCustomChoice()
    {
        var presets = ThermodynamicPresetCatalog.BuiltIn;

        Assert.Equal(
            presets.Count,
            presets.Select(static preset => preset.Id).Distinct().Count());
        Assert.Single(presets, static preset => preset.IsCustom);
        Assert.All(
            presets,
            static preset =>
            {
                Assert.False(string.IsNullOrWhiteSpace(preset.DisplayName));
                Assert.False(string.IsNullOrWhiteSpace(preset.SourceSummary));
                Assert.Equal(preset.DisplayName, preset.ToString());
            });
    }

    [Fact]
    public void BuiltIn_NonCustomValuesAreValidConstantPropertyInputs()
    {
        var presets = ThermodynamicPresetCatalog.BuiltIn
            .Where(static preset => !preset.IsCustom);

        Assert.All(
            presets,
            static preset =>
            {
                Assert.False(string.IsNullOrWhiteSpace(preset.PropellantLabel));
                Assert.True(preset.MixtureRatio > 0);
                Assert.True(preset.ChamberTemperatureKelvin > 0);
                Assert.True(preset.SpecificHeatRatio > 1);
                Assert.True(
                    preset.SpecificGasConstantJoulesPerKilogramKelvin > 0);
            });
    }

    [Fact]
    public void PublicReferenceGasConstantsMatchMolecularWeightDerivation()
    {
        var kerosene = ThermodynamicPresetCatalog.BuiltIn.Single(
            static preset =>
                preset.Id
                    == ThermodynamicPresetCatalog.LoxKeroseneReferenceId);
        var hydrogen = ThermodynamicPresetCatalog.BuiltIn.Single(
            static preset =>
                preset.Id
                    == ThermodynamicPresetCatalog.LoxHydrogenReferenceId);
        Assert.NotNull(
            kerosene.SpecificGasConstantJoulesPerKilogramKelvin);
        Assert.NotNull(
            hydrogen.SpecificGasConstantJoulesPerKilogramKelvin);

        Assert.Equal(
            356.843889191126,
            kerosene.SpecificGasConstantJoulesPerKilogramKelvin.Value,
            10);
        Assert.Equal(
            831.446261815324,
            hydrogen.SpecificGasConstantJoulesPerKilogramKelvin.Value,
            10);
    }
}
