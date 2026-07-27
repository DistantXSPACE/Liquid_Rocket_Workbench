using LiquidRocketWorkbench.Core.Models;

namespace LiquidRocketWorkbench.Core.Calculations;

/// <summary>
/// UI-independent boundary for calculating one ideal-engine operating point.
/// </summary>
public interface IEnginePerformanceCalculator
{
    EngineCalculationResult Calculate(EngineInputs inputs);
}
