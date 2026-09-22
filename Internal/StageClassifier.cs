namespace OutSystems.ExternalLibraries.RuntimeContext.Internal;

internal enum ClassificationReason
{
    SignalAbsent,
    UnrecognizedRealm,
    RecognizedRealm
}

internal readonly record struct StageClassification(
    string Classification,
    string Realm,
    bool IsClassified,
    ClassificationReason Reason)
{
    internal bool IsProduction => Classification == StageClassifier.Production;
}

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

    // SECURE_GATEWAY is shaped <host>.<realm>.econnectivity.local; the realm is the second
    // label. A value without a second label carries no signal.
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

        // Fail closed: a realm this version does not know is Unknown, never NonProduction, so a
        // future platform tier is never reported as a stage the library claims to understand.
        return new StageClassification(Unknown, realm, false, ClassificationReason.UnrecognizedRealm);
    }

    private static bool Matches(string realm, string known) =>
        string.Equals(realm, known, StringComparison.OrdinalIgnoreCase);
}
