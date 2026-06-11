using System.Runtime.InteropServices;
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
