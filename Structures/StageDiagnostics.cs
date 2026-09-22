using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.ExternalLibraries.RuntimeContext.Structures;

[OSStructure(Description = "Raw platform signals and the rule that produced the stage classification. " +
                           "Use this to diagnose an unexpected stage result.")]
public struct StageDiagnostics
{
    public StageDiagnostics()
    {
        SecureGatewaySignal = string.Empty;
        ExtractedRealm = string.Empty;
        RealmRecognized = false;
        Classification = string.Empty;
        Reason = string.Empty;
        RuntimeUrlSignal = string.Empty;
        ResolvedHost = string.Empty;
        KnownRealms = string.Empty;
    }

    [OSStructureField(Description = "Raw value of the platform signal the classification is derived from. Empty when absent.")]
    public string SecureGatewaySignal { get; set; }

    [OSStructureField(Description = "Realm token extracted from the signal, for example runp.")]
    public string ExtractedRealm { get; set; }

    [OSStructureField(Description = "True when the extracted realm matched a realm this library version knows.")]
    public bool RealmRecognized { get; set; }

    [OSStructureField(Description = "Resulting classification: Production, NonProduction, Development, or Unknown.")]
    public string Classification { get; set; }

    [OSStructureField(Description = "Why that classification was chosen: SignalAbsent, UnrecognizedRealm, or RecognizedRealm.")]
    public string Reason { get; set; }

    [OSStructureField(Description = "Raw runtime URL signal as provided by the platform. Empty when absent.")]
    public string RuntimeUrlSignal { get; set; }

    [OSStructureField(Description = "Host parsed from the runtime URL signal.")]
    public string ResolvedHost { get; set; }

    [OSStructureField(Description = "Comma-separated realm tokens this library version recognizes. " +
                                    "If ExtractedRealm is not in this list, the platform contract has changed.")]
    public string KnownRealms { get; set; }
}
