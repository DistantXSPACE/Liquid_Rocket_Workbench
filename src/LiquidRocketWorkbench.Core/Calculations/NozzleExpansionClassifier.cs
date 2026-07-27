using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// Applies POLICY-NOZZLE-01 and POLICY-NOZZLE-02 from
/// <c>docs/references.md</c>.
/// </summary>
public static class NozzleExpansionClassifier
{
    public static NozzleExpansionState Classify(
        double exitPressurePascals,
        double ambientPressurePascals)
    {
        CalculationGuard.RequireNonnegativeFinite(
            exitPressurePascals,
            nameof(exitPressurePascals));
        CalculationGuard.RequireNonnegativeFinite(
            ambientPressurePascals,
            nameof(ambientPressurePascals));

        if (exitPressurePascals == 0 && ambientPressurePascals == 0)
        {
            return NozzleExpansionState.IdeallyExpanded;
        }

        var relativeDifference =
            Math.Abs(exitPressurePascals - ambientPressurePascals)
            / Math.Max(exitPressurePascals, ambientPressurePascals);

        if (relativeDifference
            <= CalculationTolerances.NozzlePressureRelative)
        {
            return NozzleExpansionState.IdeallyExpanded;
        }

        return exitPressurePascals > ambientPressurePascals
            ? NozzleExpansionState.Underexpanded
            : NozzleExpansionState.Overexpanded;
    }
}
