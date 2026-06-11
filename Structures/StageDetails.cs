using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.ExternalLibraries.RuntimeContext.Structures;

[OSStructure(Description = "Details about the OutSystems stage the app is currently running on.")]
public struct StageDetails
{
    public StageDetails()
    {
        Classification = string.Empty;
        IsProduction = false;
        RuntimeUrl = string.Empty;
        Subdomain = string.Empty;
        InfrastructureRealm = string.Empty;
        StageId = string.Empty;
    }

    [OSStructureField(Description = "Stage type: Production, NonProduction, or Unknown.")]
    public string Classification { get; set; }

    [OSStructureField(Description = "True when the app is running on a Production stage.")]
    public bool IsProduction { get; set; }

    [OSStructureField(Description = "URL the current stage is served from, for example acme-dev.outsystems.app.")]
    public string RuntimeUrl { get; set; }

    [OSStructureField(Description = "Leading label of the runtime URL host, for example acme-dev.")]
    public string Subdomain { get; set; }

    [OSStructureField(Description = "Internal infrastructure identifier the stage type is derived from, for example runp for Production.")]
    public string InfrastructureRealm { get; set; }

    [OSStructureField(Description = "Unique identifier of the current stage.")]
    public string StageId { get; set; }
}
