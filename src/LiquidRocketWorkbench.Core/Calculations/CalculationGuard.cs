namespace LiquidRocketWorkbench.Core.Calculations;

internal static class CalculationGuard
{
    public static void RequirePositiveFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be finite and greater than zero.");
        }
    }

    public static void RequireNonnegativeFinite(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be finite and greater than or equal to zero.");
        }
    }

    public static void RequireGreaterThanOneFinite(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(value) || value <= 1)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be finite and greater than one.");
        }
    }

    public static void RequireFiniteResult(
        double value,
        string calculationName)
    {
        if (!double.IsFinite(value))
        {
            throw new OverflowException(
                $"{calculationName} produced a non-finite result.");
        }
    }

    public static void RequirePositiveFiniteResult(
        double value,
        string calculationName)
    {
        RequireFiniteResult(value, calculationName);

        if (value <= 0)
        {
            throw new ArithmeticException(
                $"{calculationName} cannot be represented as a positive "
                    + "finite value.");
        }
    }
}
