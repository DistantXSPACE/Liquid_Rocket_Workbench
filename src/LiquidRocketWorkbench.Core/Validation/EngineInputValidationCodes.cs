namespace LiquidRocketWorkbench.Core.Validation;

/// <summary>
/// Stable machine-readable codes emitted by <see cref="EngineInputsValidator"/>.
/// </summary>
public static class EngineInputValidationCodes
{
    public const string PropellantLabelRequired =
        "INPUT.PROPELLANT_LABEL.REQUIRED";
    public const string NozzleRequired = "INPUT.NOZZLE.REQUIRED";
    public const string GasRequired = "INPUT.GAS.REQUIRED";
    public const string NotFinite = "INPUT.NOT_FINITE";
    public const string MustBePositive = "INPUT.MUST_BE_POSITIVE";
    public const string MustBeNonnegative = "INPUT.MUST_BE_NONNEGATIVE";
    public const string SpecificHeatRatioMustExceedOne =
        "INPUT.GAMMA.MUST_EXCEED_ONE";
    public const string ExitDiameterSmallerThanThroat =
        "INPUT.NOZZLE.EXIT_SMALLER_THAN_THROAT";
    public const string ChamberPressureNotAboveAmbient =
        "INPUT.CHAMBER_PRESSURE.NOT_ABOVE_AMBIENT";
}
