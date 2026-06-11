# Runtime Context

An [OutSystems Developer Cloud (ODC)](https://www.outsystems.com/low-code-platform/developer-cloud/) external library that tells your app **which stage it is running on** — and in particular **whether it is production** — at runtime, with no configuration.

ODC has no built-in, runtime-readable way to know the current stage. The usual workaround is a per-stage app setting that you must configure on every stage. `Runtime Context` reads the stage tier straight from the platform instead, so it works out of the box.

## How it works

ODC external logic runs as an AWS Lambda. The platform injects a `SECURE_GATEWAY` environment variable whose host encodes the **infrastructure plane** the function runs on:

| Realm    | Tier           |
|----------|----------------|
| `runp`   | Production     |
| `runnp`  | Non-production |
| `rundev` | Development    |

`GetCurrentStage()` and `IsProductionStage()` read that realm to classify the stage.

> ⚠️ This relies on an **undocumented** platform signal, verified against the current ODC infrastructure. It could change on a platform update. The classification **fails safe** — anything that isn't clearly the production plane is reported as non-production, so it never falsely returns production. For irreversible, production-only operations, consider also gating on a per-stage app setting.

## Server actions

| Action | Returns | Description |
|--------|---------|-------------|
| `GetCurrentStage()` | `StageDetails` | Stage type, whether it's production, plus the stage id and URL. |
| `IsProductionStage()` | `Boolean` | `True` only on the production plane. |
| `GetStageId()` | `Text` | The stage's unique id (`OUTSYSTEMS_ENVIRONMENT_ID`). |
| `GetRuntimeUrl()` | `Text` | The stage's runtime URL. |
| `GetRuntimeDetails()` | `RuntimeDetails` | .NET runtime, OS, CPU, AWS region, and Lambda info. |

## Build

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet build RuntimeContext.csproj -c Release
```

## Package & upload

The icons are embedded in the assembly, so the upload package is just the DLL:

```bash
cd bin/Release/net8.0
zip RuntimeContext-ODC.zip RuntimeContext.dll
```

Then, in the ODC Portal, upload `RuntimeContext-ODC.zip` as an external library and add it as a dependency in ODC Studio.

## License

[MIT](LICENSE)
