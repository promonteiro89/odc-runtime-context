using OutSystems.ExternalLibraries.RuntimeContext.Structures;
using Xunit;

namespace OutSystems.ExternalLibraries.RuntimeContext.Tests;

// Every test that mutates the process environment lives in this one class: xUnit never runs
// tests within a single class in parallel, so they cannot race each other.
public class RuntimeContextEnvironmentTests : IDisposable
{
    private const string SecureGateway = "SECURE_GATEWAY";
    private const string RuntimeUrl = "OUTSYSTEMS_RUNTIME_URL";
    private const string XRayTraceId = "_X_AMZN_TRACE_ID";
    private const string LogGroup = "AWS_LAMBDA_LOG_GROUP_NAME";

    private readonly Dictionary<string, string?> _original = new();

    public RuntimeContextEnvironmentTests()
    {
        foreach (var name in new[] { SecureGateway, RuntimeUrl, XRayTraceId, LogGroup })
            _original[name] = Environment.GetEnvironmentVariable(name);
    }

    public void Dispose()
    {
        foreach (var (name, value) in _original)
            Environment.SetEnvironmentVariable(name, value);
    }

    private static void Set(string name, string? value) =>
        Environment.SetEnvironmentVariable(name, value);

    [Fact]
    public void XRayTraceId_IsReadFresh_NotCached()
    {
        // The Lambda runtime rewrites this on every invocation. Caching it in a static field
        // would pin every trace correlation for the worker's lifetime to the first request,
        // silently. This test fails if that regression is ever introduced.
        var sut = new RuntimeContext();

        Set(XRayTraceId, "Root=1-first-invocation");
        var first = sut.GetTraceContext().XRayTraceId;

        Set(XRayTraceId, "Root=1-second-invocation");
        var second = sut.GetTraceContext().XRayTraceId;

        Assert.Equal("Root=1-first-invocation", first);
        Assert.Equal("Root=1-second-invocation", second);
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void TraceContext_IsFreshAcrossSeparateInstances()
    {
        Set(LogGroup, "/aws/lambda/group-a");
        Assert.Equal("/aws/lambda/group-a", new RuntimeContext().GetTraceContext().LogGroupName);

        Set(LogGroup, "/aws/lambda/group-b");
        Assert.Equal("/aws/lambda/group-b", new RuntimeContext().GetTraceContext().LogGroupName);
    }

    [Fact]
    public void ProductionStage_IsDetected()
    {
        Set(SecureGateway, "gw.runp.econnectivity.local");
        Set(RuntimeUrl, "acme.outsystems.app");

        StageDetails stage = new RuntimeContext().GetCurrentStage();

        Assert.Equal("Production", stage.Classification);
        Assert.True(stage.IsProduction);
        Assert.True(stage.IsClassified);
        Assert.Equal("acme", stage.Subdomain);
        Assert.True(new RuntimeContext().IsProductionStage());
    }

    [Fact]
    public void AbsentSignal_IsUnknownAndNotProduction()
    {
        Set(SecureGateway, null);

        StageDetails stage = new RuntimeContext().GetCurrentStage();

        Assert.Equal("Unknown", stage.Classification);
        Assert.False(stage.IsClassified);
        Assert.False(stage.IsProduction);
        Assert.False(new RuntimeContext().IsProductionStage());
    }

    [Fact]
    public void UnrecognizedRealm_IsUnknown_NotNonProduction()
    {
        Set(SecureGateway, "gw.runstg.econnectivity.local");

        StageDetails stage = new RuntimeContext().GetCurrentStage();

        Assert.Equal("Unknown", stage.Classification);
        Assert.False(stage.IsClassified);
    }

    [Fact]
    public void ExplainClassification_ReportsRawSignalsAndReason()
    {
        Set(SecureGateway, "gw.runstg.econnectivity.local");
        Set(RuntimeUrl, "acme-tst.outsystems.app");

        StageDiagnostics d = new RuntimeContext().ExplainClassification();

        Assert.Equal("gw.runstg.econnectivity.local", d.SecureGatewaySignal);
        Assert.Equal("runstg", d.ExtractedRealm);
        Assert.False(d.RealmRecognized);
        Assert.Equal("Unknown", d.Classification);
        Assert.Equal("UnrecognizedRealm", d.Reason);
        Assert.Equal("acme-tst.outsystems.app", d.ResolvedHost);
        Assert.Contains("runp", d.KnownRealms);
    }

    [Fact]
    public void ExplainClassification_ReportsAbsentSignal()
    {
        Set(SecureGateway, null);

        StageDiagnostics d = new RuntimeContext().ExplainClassification();

        Assert.Equal("", d.SecureGatewaySignal);
        Assert.Equal("SignalAbsent", d.Reason);
        Assert.False(d.RealmRecognized);
    }

    [Fact]
    public void RuntimeLifecycle_ReportsFirstCallOnlyOnce()
    {
        var sut = new RuntimeContext();

        // The static flag is process-wide, so exactly one call in this process sees true.
        var results = new[]
        {
            sut.GetRuntimeLifecycle().IsFirstCallInProcess,
            sut.GetRuntimeLifecycle().IsFirstCallInProcess,
            sut.GetRuntimeLifecycle().IsFirstCallInProcess
        };

        Assert.False(results[1]);
        Assert.False(results[2]);
        Assert.True(sut.GetRuntimeLifecycle().UptimeMs >= 0);
    }

    [Fact]
    public void ActionsNeverThrow_EvenWithHostileValues()
    {
        Set(SecureGateway, ".");
        Set(RuntimeUrl, "://:::not a url");

        var sut = new RuntimeContext();

        // The SDK boundary contract: degrade, never throw.
        var ex = Record.Exception(() =>
        {
            sut.GetCurrentStage();
            sut.IsProductionStage();
            sut.ExplainClassification();
            sut.GetTraceContext();
            sut.GetRuntimeLifecycle();
            sut.GetRuntimeDetails();
            sut.GetStageId();
            sut.GetRuntimeUrl();
        });

        Assert.Null(ex);
    }
}
