using OutSystems.ExternalLibraries.SDK;
using OutSystems.ExternalLibraries.RuntimeContext.Structures;

namespace OutSystems.ExternalLibraries.RuntimeContext;

[OSInterface(
    Description = "Provides information about the OutSystems stage and server runtime the app is running on, including whether it is a Production stage.",
    Name = "RuntimeContext",
    IconResourceName = "OutSystems.ExternalLibraries.RuntimeContext.Resources.app_icon.png")]
public interface IRuntimeContext
{
    [OSAction(
        Description = "Returns details about the current stage: its type (Production, NonProduction, or Unknown), identifier, and URL.",
        ReturnName = "Stage",
        ReturnDescription = "Details about the current stage (type, identifier, and URL).",
        IconResourceName = "OutSystems.ExternalLibraries.RuntimeContext.Resources.action_icon.png")]
    StageDetails GetCurrentStage();

    [OSAction(
        Description = "Returns True when the app is running on a Production stage.",
        ReturnName = "IsProduction",
        ReturnDescription = "True when running on a Production stage; otherwise False.",
        IconResourceName = "OutSystems.ExternalLibraries.RuntimeContext.Resources.action_icon.png")]
    bool IsProductionStage();

    [OSAction(
        Description = "Returns the unique identifier of the current stage.",
        ReturnName = "StageId",
        ReturnDescription = "Unique identifier of the current stage.",
        IconResourceName = "OutSystems.ExternalLibraries.RuntimeContext.Resources.action_icon.png")]
    string GetStageId();

    [OSAction(
        Description = "Returns the URL the current stage is served from.",
        ReturnName = "RuntimeUrl",
        ReturnDescription = "URL the current stage is served from.",
        IconResourceName = "OutSystems.ExternalLibraries.RuntimeContext.Resources.action_icon.png")]
    string GetRuntimeUrl();

    [OSAction(
        Description = "Returns technical details about the server runtime, such as the .NET version, operating system, CPU, and region.",
        ReturnName = "Runtime",
        ReturnDescription = "Technical details about the server runtime (framework, OS, CPU, region).",
        IconResourceName = "OutSystems.ExternalLibraries.RuntimeContext.Resources.action_icon.png")]
    RuntimeDetails GetRuntimeDetails();
}
