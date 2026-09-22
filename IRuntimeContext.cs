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
        Description = "Returns details about the current stage: its type (Production, NonProduction, Development, or Unknown), identifier, and URL.",
        ReturnName = "Stage",
        ReturnDescription = "Details about the current stage (type, identifier, and URL).",
        IconResourceName = "OutSystems.ExternalLibraries.RuntimeContext.Resources.action_icon.png")]
    StageDetails GetCurrentStage();

    [OSAction(
        Description = "Returns True only when the app is confirmed to be running on a Production stage. " +
                      "Returns False when the stage is Unknown, so use it to guard production-only logic — " +
                      "never to enable non-production behavior.",
        ReturnName = "IsProduction",
        ReturnDescription = "True when running on a Production stage; otherwise False.",
        IconResourceName = "OutSystems.ExternalLibraries.RuntimeContext.Resources.action_icon.png")]
    bool IsProductionStage();

    [OSAction(
        Description = "Returns the raw platform signals read and the rule that produced the stage classification. " +
                      "Use this when stage detection returns an unexpected result.",
        ReturnName = "Diagnostics",
        ReturnDescription = "Raw signals, extracted realm, and the reason behind the classification.",
        IconResourceName = "OutSystems.ExternalLibraries.RuntimeContext.Resources.action_icon.png")]
    StageDiagnostics ExplainClassification();

    [OSAction(
        Description = "Returns the underlying AWS trace and log context, for correlating ODC logs with " +
                      "infrastructure traces in third-party monitoring tools.",
        ReturnName = "Trace",
        ReturnDescription = "X-Ray trace identifier and CloudWatch log group and stream.",
        IconResourceName = "OutSystems.ExternalLibraries.RuntimeContext.Resources.action_icon.png")]
    TraceContext GetTraceContext();

    [OSAction(
        Description = "Returns facts about the lifetime of the runtime process serving this library, " +
                      "such as how long it has been loaded and whether this is the first call to it.",
        ReturnName = "Lifecycle",
        ReturnDescription = "Process uptime and whether this is the first call within the process.",
        IconResourceName = "OutSystems.ExternalLibraries.RuntimeContext.Resources.action_icon.png")]
    RuntimeLifecycle GetRuntimeLifecycle();

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
