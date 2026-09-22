using OutSystems.ExternalLibraries.RuntimeContext.Internal;
using Xunit;

namespace OutSystems.ExternalLibraries.RuntimeContext.Tests;

/// Encodes the realm-to-tier contract observed on a live ODC tenant across all four stages
/// (eu-central-1, 2026-09-22). These are observations of an undocumented platform contract,
/// not a guarantee — if ODC changes it, these tests are where it should surface.
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
        // Verified on the live tenant: both stages report "runnp". Any future proposal to map
        // runnp to a finer tier (Test vs UAT vs Pre-Production) is fabricating resolution the
        // signal does not carry. This test exists to make that impossible to forget.
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
