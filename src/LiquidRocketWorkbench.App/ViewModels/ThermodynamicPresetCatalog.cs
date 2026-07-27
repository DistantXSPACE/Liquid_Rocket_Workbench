namespace LiquidRocketWorkbench.App.ViewModels;

/// <summary>
/// Built-in, sourced starting points for the constant-property model.
/// </summary>
public static class ThermodynamicPresetCatalog
{
    private const double UniversalGasConstantJoulesPerKilomoleKelvin =
        8314.46261815324;

    public const string CustomId = "custom";

    public const string ReferenceCaseId = "vc-04-reference";

    public const string LoxMethaneNasaTestId = "lox-methane-nasa-test";

    public const string LoxKeroseneReferenceId = "lox-kerosene-reference";

    public const string LoxHydrogenReferenceId = "lox-hydrogen-reference";

    public static IReadOnlyList<ThermodynamicPreset> BuiltIn { get; } =
        Array.AsReadOnly(
            [
                new ThermodynamicPreset(
                    CustomId,
                    "Custom / keep current values",
                    "Custom",
                    null,
                    null,
                    null,
                    null,
                    "No preset is applied. Enter a descriptive propellant "
                        + "label and provide all thermodynamic values."),
                new ThermodynamicPreset(
                    ReferenceCaseId,
                    "Synthetic reference gas · VC-04",
                    "VC-04 synthetic constant-property gas",
                    3.5,
                    3500,
                    1.22,
                    355,
                    "Project validation case VC-04. This synthetic gas keeps "
                        + "the documented equation-chain reference values and "
                        + "does not represent combustion chemistry."),
                new ThermodynamicPreset(
                    LoxMethaneNasaTestId,
                    "LOX / Methane · NASA test condition",
                    "LOX / Methane",
                    3.48,
                    6560 * 5.0 / 9.0,
                    1.182,
                    CalculateGasConstantFromSoundSpeed(
                        speedOfSoundInchesPerSecond: 51819,
                        specificHeatRatio: 1.182,
                        temperatureRankine: 6560),
                    "NASA TM-102381 table II, MSFC LOX/methane condition. "
                        + "R is derived from the listed sound speed using "
                        + "a² = gamma R T. Treat these as editable, "
                        + "constant-property inputs for that test condition."),
                new ThermodynamicPreset(
                    LoxKeroseneReferenceId,
                    "LOX / Kerosene · public reference",
                    "LOX / Kerosene (reference estimate)",
                    2.56,
                    3670,
                    1.24,
                    CalculateGasConstantFromMolecularWeight(23.30),
                    "Astronautix LOX/kerosene reference values. R is derived "
                        + "from the listed molecular weight. This is an "
                        + "editable comparison estimate, not an engine-specific "
                        + "chemistry solution."),
                new ThermodynamicPreset(
                    LoxHydrogenReferenceId,
                    "LOX / Hydrogen · public reference",
                    "LOX / Hydrogen (reference estimate)",
                    6,
                    2985,
                    1.26,
                    CalculateGasConstantFromMolecularWeight(10),
                    "Astronautix LOX/LH2 reference values. R is derived from "
                        + "the listed molecular weight. This is an editable "
                        + "comparison estimate, not an engine-specific "
                        + "chemistry solution."),
            ]);

    public static ThermodynamicPreset Default =>
        BuiltIn.Single(
            static preset => preset.Id == ReferenceCaseId);

    private static double CalculateGasConstantFromSoundSpeed(
        double speedOfSoundInchesPerSecond,
        double specificHeatRatio,
        double temperatureRankine)
    {
        var speedOfSoundMetersPerSecond =
            speedOfSoundInchesPerSecond * 0.0254;
        var temperatureKelvin = temperatureRankine * 5 / 9;

        return speedOfSoundMetersPerSecond
            * speedOfSoundMetersPerSecond
            / (specificHeatRatio * temperatureKelvin);
    }

    private static double CalculateGasConstantFromMolecularWeight(
        double kilogramsPerKilomole)
    {
        return UniversalGasConstantJoulesPerKilomoleKelvin
            / kilogramsPerKilomole;
    }
}
