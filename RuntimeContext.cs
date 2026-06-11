using System;
using System.Runtime.InteropServices;
using OutSystems.ExternalLibraries.SDK;

namespace RuntimeContext;

// ════════════════════════ Structures ════════════════════════

[OSStructure(Description = "Details about the OutSystems stage the app is currently running on.")]
public struct StageDetails
{
    [OSStructureField(Description = "Stage type: Production, NonProduction, or Unknown.")]
    public string Classification;

    [OSStructureField(Description = "True when the app is running on a Production stage.")]
    public bool IsProduction;

    [OSStructureField(Description = "URL the current stage is served from, for example acme-dev.outsystems.app.")]
    public string RuntimeUrl;

    [OSStructureField(Description = "Leading label of the runtime URL host, for example acme-dev.")]
    public string Subdomain;

    [OSStructureField(Description = "Internal infrastructure identifier the stage type is derived from, for example runp for Production.")]
    public string InfrastructureRealm;

    [OSStructureField(Description = "Unique identifier of the current stage.")]
    public string StageId;
}

[OSStructure(Description = "Technical details about the server runtime that executes the external logic.")]
public struct RuntimeDetails
{
    [OSStructureField(Description = ".NET runtime version, for example .NET 8.0.x.")]
    public string DotNetVersion;

    [OSStructureField(Description = "Operating system the runtime runs on, for example Amazon Linux.")]
    public string OperatingSystem;

    [OSStructureField(Description = "Host name of the server instance.")]
    public string MachineName;

    [OSStructureField(Description = "Number of logical processors available.")]
    public int ProcessorCount;

    [OSStructureField(Description = "True if the operating system is 64-bit.")]
    public bool Is64BitOS;

    [OSStructureField(Description = "True if the running process is 64-bit.")]
    public bool Is64BitProcess;

    [OSStructureField(Description = "Cloud region the runtime is hosted in, for example us-east-1.")]
    public string AwsRegion;

    [OSStructureField(Description = "Name of the underlying serverless function.")]
    public string LambdaFunctionName;

    [OSStructureField(Description = "Memory allocated to the runtime, in MB.")]
    public int LambdaMemoryMB;
}

// ════════════════════════ Interface ════════════════════════

[OSInterface(
    Name = "RuntimeContext",
    Description = "Provides information about the OutSystems stage and server runtime the app is running on, including whether it is a Production stage.",
    IconResourceName = "app-icon.png")]
public interface IRuntimeContext
{
    [OSAction(Description = "Returns details about the current stage: its type (Production, NonProduction, or Unknown), identifier, and URL.", ReturnName = "Stage", ReturnDescription = "Details about the current stage (type, identifier, and URL).", IconResourceName = "action-icon.png")]
    StageDetails GetCurrentStage();

    [OSAction(Description = "Returns True when the app is running on a Production stage.", ReturnName = "IsProduction", ReturnDescription = "True when running on a Production stage; otherwise False.", IconResourceName = "action-icon.png")]
    bool IsProductionStage();

    [OSAction(Description = "Returns the unique identifier of the current stage.", ReturnName = "StageId", ReturnDescription = "Unique identifier of the current stage.", IconResourceName = "action-icon.png")]
    string GetStageId();

    [OSAction(Description = "Returns the URL the current stage is served from.", ReturnName = "RuntimeUrl", ReturnDescription = "URL the current stage is served from.", IconResourceName = "action-icon.png")]
    string GetRuntimeUrl();

    [OSAction(Description = "Returns technical details about the server runtime, such as the .NET version, operating system, CPU, and region.", ReturnName = "Runtime", ReturnDescription = "Technical details about the server runtime (framework, OS, CPU, region).", IconResourceName = "action-icon.png")]
    RuntimeDetails GetRuntimeDetails();
}

// ════════════════════════ Implementation ════════════════════════

public sealed class RuntimeContextService : IRuntimeContext
{
    private const string EnvStageId = "OUTSYSTEMS_ENVIRONMENT_ID";
    private const string EnvRuntimeUrl = "OUTSYSTEMS_RUNTIME_URL";
    private const string EnvSecureGateway = "SECURE_GATEWAY";
    private const string EnvAwsRegion = "AWS_REGION";
    private const string EnvLambdaName = "AWS_LAMBDA_FUNCTION_NAME";
    private const string EnvLambdaMemory = "AWS_LAMBDA_FUNCTION_MEMORY_SIZE";
    private const string ProductionRealm = "runp";

    public StageDetails GetCurrentStage()
    {
        var realm = CurrentRealm();
        var url = Env(EnvRuntimeUrl);

        // SECURE_GATEWAY's host carries the infrastructure identifier:
        //   runp   = Production
        //   runnp  = Non-production (test, pre-prod, …)
        //   rundev = Development
        string classification =
            string.IsNullOrEmpty(realm) ? "Unknown" :
            IsProductionRealm(realm) ? "Production" :
            "NonProduction";

        return new StageDetails
        {
            Classification = classification,
            IsProduction = classification == "Production",
            RuntimeUrl = url,
            Subdomain = SubdomainOf(url),
            InfrastructureRealm = realm,
            StageId = Env(EnvStageId)
        };
    }

    public bool IsProductionStage() => IsProductionRealm(CurrentRealm());

    public string GetStageId() => Env(EnvStageId);

    public string GetRuntimeUrl() => Env(EnvRuntimeUrl);

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

    // The production check lives here so GetCurrentStage and IsProductionStage stay consistent.
    private static string CurrentRealm() => ExtractInfraRealm(Env(EnvSecureGateway));

    private static bool IsProductionRealm(string realm) =>
        string.Equals(realm, ProductionRealm, StringComparison.OrdinalIgnoreCase);

    private static string SubdomainOf(string url)
    {
        if (string.IsNullOrEmpty(url)) return "";
        var host = url;
        int scheme = host.IndexOf("//", StringComparison.Ordinal);
        if (scheme >= 0) host = host.Substring(scheme + 2);
        int slash = host.IndexOf('/');
        if (slash >= 0) host = host.Substring(0, slash);
        int dot = host.IndexOf('.');
        return dot > 0 ? host.Substring(0, dot) : host;       // "acme-dev.outsystems.app" -> "acme-dev"
    }

    private static string ExtractInfraRealm(string secureGateway)
    {
        if (string.IsNullOrEmpty(secureGateway)) return "";
        var parts = secureGateway.Split('.');                 // <host>.<realm>.econnectivity.local
        return parts.Length >= 2 ? parts[1] : "";
    }

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
