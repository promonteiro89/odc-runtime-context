using System.Runtime.InteropServices;
using OutSystems.ExternalLibraries.RuntimeContext.Internal;
using OutSystems.ExternalLibraries.RuntimeContext.Structures;

namespace OutSystems.ExternalLibraries.RuntimeContext;

public class RuntimeContext : IRuntimeContext
{
    private const string EnvStageId = "OUTSYSTEMS_ENVIRONMENT_ID";
    private const string EnvRuntimeUrl = "OUTSYSTEMS_RUNTIME_URL";
    private const string EnvSecureGateway = "SECURE_GATEWAY";
    private const string EnvAwsRegion = "AWS_REGION";
    private const string EnvLambdaName = "AWS_LAMBDA_FUNCTION_NAME";
    private const string EnvLambdaMemory = "AWS_LAMBDA_FUNCTION_MEMORY_SIZE";
    private const string EnvXRayTraceId = "_X_AMZN_TRACE_ID";
    private const string EnvLogGroup = "AWS_LAMBDA_LOG_GROUP_NAME";
    private const string EnvLogStream = "AWS_LAMBDA_LOG_STREAM_NAME";

    private static readonly long LoadedAtTicks = Environment.TickCount64;
    private static int _firstCallPending = 1;

    public StageDetails GetCurrentStage()
    {
        var stage = StageClassifier.Classify(Env(EnvSecureGateway));
        var url = Env(EnvRuntimeUrl);

        return new StageDetails
        {
            Classification = stage.Classification,
            IsProduction = stage.IsProduction,
            IsClassified = stage.IsClassified,
            RuntimeUrl = url,
            Subdomain = HostParser.SubdomainOf(url),
            InfrastructureRealm = stage.Realm,
            StageId = Env(EnvStageId)
        };
    }

    public bool IsProductionStage() => StageClassifier.Classify(Env(EnvSecureGateway)).IsProduction;

    public string GetStageId() => Env(EnvStageId);

    public string GetRuntimeUrl() => Env(EnvRuntimeUrl);

    public StageDiagnostics ExplainClassification()
    {
        var signal = Env(EnvSecureGateway);
        var url = Env(EnvRuntimeUrl);
        var stage = StageClassifier.Classify(signal);

        return new StageDiagnostics
        {
            SecureGatewaySignal = signal,
            ExtractedRealm = stage.Realm,
            RealmRecognized = stage.IsClassified,
            Classification = stage.Classification,
            Reason = stage.Reason.ToString(),
            RuntimeUrlSignal = url,
            ResolvedHost = HostParser.HostOf(url),
            KnownRealms = string.Join(", ", StageClassifier.KnownRealms)
        };
    }

    public TraceContext GetTraceContext()
    {
        return new TraceContext
        {
            // The trace id is rewritten by the Lambda runtime on every invocation. Caching it
            // would pin every correlation for the life of the worker to the first request.
            XRayTraceId = Env(EnvXRayTraceId),
            LogGroupName = Env(EnvLogGroup),
            LogStreamName = Env(EnvLogStream)
        };
    }

    public RuntimeLifecycle GetRuntimeLifecycle()
    {
        return new RuntimeLifecycle
        {
            UptimeMs = Environment.TickCount64 - LoadedAtTicks,
            IsFirstCallInProcess = Interlocked.Exchange(ref _firstCallPending, 0) == 1
        };
    }

    public RuntimeDetails GetRuntimeDetails()
    {
        return new RuntimeDetails
        {
            DotNetVersion = Safe(() => RuntimeInformation.FrameworkDescription),
            OperatingSystem = Safe(() => RuntimeInformation.OSDescription),
            MachineName = Safe(() => Environment.MachineName),
            ProcessorCount = SafeInt(() => Environment.ProcessorCount),
            Is64BitOS = SafeBool(() => Environment.Is64BitOperatingSystem),
            Is64BitProcess = SafeBool(() => Environment.Is64BitProcess),
            AwsRegion = Env(EnvAwsRegion),
            LambdaFunctionName = Env(EnvLambdaName),
            LambdaMemoryMB = ParseInt(Env(EnvLambdaMemory))
        };
    }

    // ───── helpers ─────

    // External logic must never throw across the SDK boundary; these wrappers degrade to safe defaults.
    private static string Env(string name)
    {
        try { return Environment.GetEnvironmentVariable(name) ?? ""; } catch { return ""; }
    }

    private static int ParseInt(string s) => int.TryParse(s, out var v) ? v : 0;

    private static string Safe(Func<string> f) { try { return f() ?? ""; } catch { return ""; } }
    private static int SafeInt(Func<int> f) { try { return f(); } catch { return 0; } }
    private static bool SafeBool(Func<bool> f) { try { return f(); } catch { return false; } }
}
