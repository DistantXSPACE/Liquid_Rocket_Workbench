namespace LiquidRocketWorkbench.Core.Calculations;

internal static class CalculationMath
{
    public static double LogOnePlus(double value)
    {
        if (Math.Abs(value) > 1e-4)
        {
            return Math.Log(1 + value);
        }

        var valueSquared = value * value;
        return value
            - (valueSquared / 2)
            + (valueSquared * value / 3)
            - (valueSquared * valueSquared / 4);
    }

    public static double LogOnePlusFromLog(double logValue)
    {
        if (logValue > 0)
        {
            return logValue + LogOnePlus(Math.Exp(-logValue));
        }

        return LogOnePlus(Math.Exp(logValue));
    }
}
