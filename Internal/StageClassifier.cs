namespace OutSystems.ExternalLibraries.RuntimeContext.Internal;

internal enum ClassificationReason
{
    /// No stage signal was present in the environment.
    SignalAbsent,

    /// A signal was present but its realm token is not one this version knows.
    UnrecognizedRealm,

    /// A signal was present and its realm token mapped to a known tier.
    RecognizedRealm
}

/// Immutable here on purpose: this type never crosses the ODC SDK boundary,
/// so it costs nothing. The [OSStructure] DTOs must stay mutable for the SDK.
internal readonly record struct StageClassification(
    string Classification,
    string Realm,
    bool IsClassified,
    ClassificationReason Reason)
{
    internal bool IsProduction => Classification == StageClassifier.Production;
}

/// Pure stage-classification logic, deliberately free of any environment access
/// so it can be unit tested without mocks, interfaces, or process mutation.
internal static class StageClassifier
{
    internal const string Production = "Production";
    internal const string NonProduction = "NonProduction";
    internal const string Development = "Development";
    internal const string Unknown = "Unknown";

    internal const string ProductionRealm = "runp";
    internal const string NonProductionRealm = "runnp";
    internal const string DevelopmentRealm = "rundev";

    internal static readonly string[] KnownRealms =
        [ProductionRealm, NonProductionRealm, DevelopmentRealm];

    /// SECURE_GATEWAY is shaped "&lt;host&gt;.&lt;realm&gt;.econnectivity.local"; the realm is the
    /// second label. Anything that does not have a second label carries no signal.
    internal static string ExtractRealm(string? secureGateway)
    {
        if (string.IsNullOrWhiteSpace(secureGateway)) return "";

        var parts = secureGateway.Trim().Split('.');
        return parts.Length >= 2 ? parts[1].Trim() : "";
    }

    internal static StageClassification Classify(string? secureGateway)
    {
        var realm = ExtractRealm(secureGateway);

        if (realm.Length == 0)
            return new StageClassification(Unknown, realm, false, ClassificationReason.SignalAbsent);

        if (Matches(realm, ProductionRealm))
            return new StageClassification(Production, realm, true, ClassificationReason.RecognizedRealm);

        if (Matches(realm, DevelopmentRealm))
            return new StageClassification(Development, realm, true, ClassificationReason.RecognizedRealm);

        if (Matches(realm, NonProductionRealm))
            return new StageClassification(NonProduction, realm, true, ClassificationReason.RecognizedRealm);

        // Fail closed. A realm token this version does not know is Unknown, never NonProduction —
        // a future platform tier must not be silently reported as a stage we understand.
        return new StageClassification(Unknown, realm, false, ClassificationReason.UnrecognizedRealm);
    }

    private static bool Matches(string realm, string known) =>
        string.Equals(realm, known, StringComparison.OrdinalIgnoreCase);
}
