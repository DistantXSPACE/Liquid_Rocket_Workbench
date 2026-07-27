namespace LiquidRocketWorkbench.Core.Tests;

public sealed class PhysicalConstantsTests
{
    [Fact]
    public void StandardGravity_MatchesNistValue()
    {
        Assert.Equal(
            9.80665,
            PhysicalConstants.StandardGravityMetersPerSecondSquared);
    }

    [Fact]
    public void StandardSeaLevelPressure_MatchesUsStandardAtmosphere()
    {
        Assert.Equal(
            101_325,
            PhysicalConstants.StandardSeaLevelPressurePascals);
    }
}
