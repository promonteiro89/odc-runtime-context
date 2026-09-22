using OutSystems.ExternalLibraries.RuntimeContext.Internal;
using Xunit;

namespace OutSystems.ExternalLibraries.RuntimeContext.Tests;

public class StageClassifierTests
{
    private const string Gateway = "somehost.{0}.econnectivity.local";

    private static string GatewayFor(string realm) => string.Format(Gateway, realm);

    [Theory]
    [InlineData("runp", "Production")]
    [InlineData("runnp", "NonProduction")]
    [InlineData("rundev", "Development")]
    public void KnownRealms_MapToTheirTier(string realm, string expected)
    {
        var result = StageClassifier.Classify(GatewayFor(realm));

        Assert.Equal(expected, result.Classification);
        Assert.True(result.IsClassified);
        Assert.Equal(ClassificationReason.RecognizedRealm, result.Reason);
    }

    [Theory]
    [InlineData("RUNP")]
    [InlineData("RunP")]
    public void RealmMatching_IsCaseInsensitive(string realm)
    {
        var result = StageClassifier.Classify(GatewayFor(realm));

        Assert.Equal("Production", result.Classification);
        Assert.True(result.IsProduction);
    }

    [Fact]
    public void Development_IsNotCollapsedIntoNonProduction()
    {
        // Regression guard: the realm distinguishes dev from other non-prod tiers.
        var result = StageClassifier.Classify(GatewayFor("rundev"));

        Assert.Equal("Development", result.Classification);
        Assert.NotEqual("NonProduction", result.Classification);
    }

    [Theory]
    [InlineData("runstg")]   // a tier this version does not know
    [InlineData("runp2")]
    [InlineData("prod")]
    public void UnrecognizedRealm_FailsClosedToUnknown_NotNonProduction(string realm)
    {
        var result = StageClassifier.Classify(GatewayFor(realm));

        Assert.Equal("Unknown", result.Classification);
        Assert.False(result.IsClassified);
        Assert.False(result.IsProduction);
        Assert.Equal(ClassificationReason.UnrecognizedRealm, result.Reason);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("nodots")]           // no second label, so no realm
    public void AbsentOrMalformedSignal_IsUnknownAndUnclassified(string? gateway)
    {
        var result = StageClassifier.Classify(gateway);

        Assert.Equal("Unknown", result.Classification);
        Assert.False(result.IsClassified);
        Assert.False(result.IsProduction);
        Assert.Equal(ClassificationReason.SignalAbsent, result.Reason);
    }

    [Fact]
    public void UnknownIsNeverReportedAsProduction()
    {
        // The core safety property: no signal must never yield a Production positive.
        foreach (var gateway in new string?[] { null, "", "nodots", GatewayFor("runstg") })
            Assert.False(StageClassifier.Classify(gateway).IsProduction);
    }

    [Fact]
    public void ExtractRealm_TakesTheSecondLabel()
    {
        Assert.Equal("runp", StageClassifier.ExtractRealm("gw.runp.econnectivity.local"));
        Assert.Equal("runnp", StageClassifier.ExtractRealm("gw.runnp.econnectivity.local"));
    }
}
