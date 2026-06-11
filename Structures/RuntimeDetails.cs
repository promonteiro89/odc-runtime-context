using OutSystems.ExternalLibraries.SDK;

namespace OutSystems.ExternalLibraries.RuntimeContext.Structures;

[OSStructure(Description = "Technical details about the server runtime that executes the external logic.")]
public struct RuntimeDetails
{
    public RuntimeDetails()
    {
        DotNetVersion = string.Empty;
        OperatingSystem = string.Empty;
        MachineName = string.Empty;
        ProcessorCount = 0;
        Is64BitOS = false;
        Is64BitProcess = false;
        AwsRegion = string.Empty;
        LambdaFunctionName = string.Empty;
        LambdaMemoryMB = 0;
    }

    [OSStructureField(Description = ".NET runtime version, for example .NET 8.0.x.")]
    public string DotNetVersion { get; set; }

    [OSStructureField(Description = "Operating system the runtime runs on, for example Amazon Linux.")]
    public string OperatingSystem { get; set; }

    [OSStructureField(Description = "Host name of the server instance.")]
    public string MachineName { get; set; }

    [OSStructureField(Description = "Number of logical processors available.")]
    public int ProcessorCount { get; set; }

    [OSStructureField(Description = "True if the operating system is 64-bit.")]
    public bool Is64BitOS { get; set; }

    [OSStructureField(Description = "True if the running process is 64-bit.")]
    public bool Is64BitProcess { get; set; }

    [OSStructureField(Description = "Cloud region the runtime is hosted in, for example us-east-1.")]
    public string AwsRegion { get; set; }

    [OSStructureField(Description = "Name of the underlying serverless function.")]
    public string LambdaFunctionName { get; set; }

    [OSStructureField(Description = "Memory allocated to the runtime, in MB.")]
    public int LambdaMemoryMB { get; set; }
}
