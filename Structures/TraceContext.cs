using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.ExternalLibraries.RuntimeContext.Structures;

[OSStructure(Description = "Underlying AWS trace and log context, for correlating ODC logs with " +
                           "infrastructure traces in third-party monitoring tools.")]
public struct TraceContext
{
    public TraceContext()
    {
        XRayTraceId = string.Empty;
        LogGroupName = string.Empty;
        LogStreamName = string.Empty;
    }

    [OSStructureField(Description = "AWS X-Ray trace identifier for the current invocation. " +
                                    "Changes on every invocation — never cache this value.")]
    public string XRayTraceId { get; set; }

    [OSStructureField(Description = "CloudWatch log group backing the current runtime.")]
    public string LogGroupName { get; set; }

    [OSStructureField(Description = "CloudWatch log stream backing the current runtime instance.")]
    public string LogStreamName { get; set; }
}
