using OutSystems.ExternalLibraries.RuntimeContext.Internal;
using Xunit;

namespace OutSystems.ExternalLibraries.RuntimeContext.Tests;

// The realm-to-tier mapping observed on a live ODC tenant across all four stages
// (eu-central-1, 2026-09-22). The platform contract is undocumented, so if it changes
// these tests are where it surfaces.
public class PlatformContractTests
{
    [Theory]
    [InlineData("Development", "rundev", "Development", false)]
    [InlineData("Testing", "runnp", "NonProduction", false)]
    [InlineData("Pre-Production", "runnp", "NonProduction", false)]
    [InlineData("Production", "runp", "Production", true)]
    public void ObservedStageMatrix_ClassifiesAsVerified(
        string stageName, string realm, string expectedClassification, bool expectedIsProduction)
    {
        var result = StageClassifier.Classify($"gw.{realm}.econnectivity.local");

        Assert.Equal(expectedClassification, result.Classification);
        Assert.Equal(expectedIsProduction, result.IsProduction);
        Assert.True(result.IsClassified, $"{stageName} must classify positively.");
    }

    [Fact]
    public void TestingAndPreProduction_ShareTheSameRealm_SoAreIndistinguishable()
    {
        // Both stages report "runnp" on the live tenant, so mapping runnp to a finer tier
        // (Test vs UAT vs Pre-Production) would invent resolution the signal does not carry.
        var testing = StageClassifier.Classify("gw.runnp.econnectivity.local");
        var preProduction = StageClassifier.Classify("gw.runnp.econnectivity.local");

        Assert.Equal(testing.Classification, preProduction.Classification);
        Assert.Equal("NonProduction", testing.Classification);
    }

    [Fact]
    public void ProductionIsTheOnlyRealmThatYieldsIsProduction()
    {
        foreach (var realm in new[] { "rundev", "runnp" })
            Assert.False(StageClassifier.Classify($"gw.{realm}.econnectivity.local").IsProduction);

        Assert.True(StageClassifier.Classify("gw.runp.econnectivity.local").IsProduction);
    }
}
