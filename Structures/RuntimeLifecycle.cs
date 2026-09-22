using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.ExternalLibraries.RuntimeContext.Structures;

[OSStructure(Description = "Facts about the lifetime of the runtime process serving this library. " +
                           "Useful for reasoning about warm reuse of the underlying serverless worker.")]
public struct RuntimeLifecycle
{
    public RuntimeLifecycle()
    {
        UptimeMs = 0;
        IsFirstCallInProcess = false;
    }

    [OSStructureField(Description = "Milliseconds since this library was first loaded in the current process. " +
                                    "A small value suggests a recently started worker; it is not an exact container age.")]
    public long UptimeMs { get; set; }

    [OSStructureField(Description = "True only on the first call to this library within the current process. " +
                                    "This is not the same as the app's cold start — the library may first be " +
                                    "called long after the worker started.")]
    public bool IsFirstCallInProcess { get; set; }
}
