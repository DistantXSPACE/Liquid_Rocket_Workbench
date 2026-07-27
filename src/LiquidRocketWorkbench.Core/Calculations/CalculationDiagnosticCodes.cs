namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Stable machine-readable codes for calculation failures.
/// </summary>
public static class CalculationDiagnosticCodes
{
    public const string AreaMachRootNotBracketed =
        "CALCULATION.AREA_MACH.ROOT_NOT_BRACKETED";
    public const string AreaMachIterationLimitReached =
        "CALCULATION.AREA_MACH.ITERATION_LIMIT_REACHED";
    public const string NumericFailure =
        "CALCULATION.NUMERIC_FAILURE";
    public const string IdealFlowAssumptions =
        "MODEL.IDEAL_FLOW_ASSUMPTIONS";
    public const string SevereOverexpansionLimit =
        "MODEL.NOZZLE.SEVERE_OVEREXPANSION_LIMIT";
    public const string TargetMassFlowMismatch =
        "MODEL.MASS_FLOW.TARGET_MISMATCH";
}
