# Runtime Context for ODC

[![Platform](https://img.shields.io/badge/Platform-OutSystems_ODC-red.svg)](https://www.outsystems.com/odc/)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Dependencies](https://img.shields.io/badge/Dependencies-None-brightgreen.svg)](#)

A lightweight .NET 10.0 External Logic component for OutSystems Developer Cloud (ODC) that lets an app discover **which stage it is running on** — and in particular **whether it is Production** — at runtime, with zero configuration.

> [!WARNING]
> **`IsProductionStage()` is fail-closed and directional.** It returns `False` both when the stage
> is confirmed non-production **and** when the stage could not be determined at all.
>
> ✅ **Safe:** `If IsProductionStage() Then <production-only behavior>`
> ❌ **Unsafe:** `If Not IsProductionStage() Then <test behavior, mock gateways, seed data>`
>
> The unsafe pattern routes **real production traffic** into test behavior on any stage where the
> platform signal is missing. When the distinction matters, read `GetCurrentStage()` and branch on
> **`IsClassified`** — it is `True` only when the stage was positively identified.

## Table of Contents

- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Action Reference](#action-reference)
- [Data Structures](#data-structures)
- [Project Structure](#project-structure)
- [Build and Deployment](#build-and-deployment)
- [Notes and Best Practices](#notes-and-best-practices)
- [Contributing](#contributing)
- [License](#license)

---

## Architecture

```
RuntimeContext/
├── RuntimeContext.csproj   # Project definition
├── IRuntimeContext.cs      # ODC External Logic interface
├── RuntimeContext.cs       # Implementation
├── Resources/              # Embedded branded icons
└── Structures/             # Strongly-typed ODC structures
```

The library is a **stateless reader**. ODC has no built-in, runtime-readable way to know the current stage; the usual workaround is a per-stage app setting that must be configured on every stage. Runtime Context instead reads the stage tier from the platform-injected environment of the external-logic runtime, so it works out of the box.

### Key Architectural Decisions
- **Stateless execution:** every action reads the environment on demand; there is no shared state, ensuring thread safety in high-concurrency ODC environments. Environment values are deliberately **not cached** — `_X_AMZN_TRACE_ID` changes on every invocation, and caching it would pin every trace correlation for the life of the worker to the first request.
- **Fails closed:** anything not positively identified as Production is reported as `Unknown` or non-production, so the component never returns a false positive for Production. An unrecognized realm token yields `Unknown`, never a confident wrong tier.
- **Pure logic, thin I/O:** classification and parsing are pure functions over plain strings, so they are testable without mocks or DI — which ODC External Logic does not support anyway.
- **Never throws across the SDK boundary:** a diagnostic utility must not be able to abort the calling OutSystems action. Missing signals degrade to empty values, surfaced honestly via `IsClassified`.
- **Resource embedding:** branded icons are embedded directly into the assembly for an integrated experience in ODC Studio.

---

## Prerequisites

- [OutSystems Developer Cloud (ODC)](https://www.outsystems.com/odc/)
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

No third-party packages — the library depends only on the OutSystems External Libraries SDK (compile-time) and the .NET base class library.

---

## Quick Start

```bash
# Build
dotnet build RuntimeContext.csproj -c Release

# Publish for ODC
dotnet publish RuntimeContext.csproj -c Release -f net10.0 --no-self-contained
```

After publishing, zip the contents of the `publish/` folder (**excluding** `OutSystems.ExternalLibraries.SDK.dll`) and upload it to the ODC Portal under **External Logic**.

---

## Action Reference

All actions take **no input parameters** — call them anywhere in server-side logic and read the outputs.

#### `GetCurrentStage`
Returns the classification and context of the stage the app is running on.

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `Stage` | `StageDetails` | Stage type, whether it is production, identifier, and URL |

#### `IsProductionStage`
Quick boolean check for guarding production-only logic. Fail-closed — see the warning at the top of this document before using it.

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `IsProduction` | `Boolean` | True only when positively identified as a Production stage |

#### `ExplainClassification`
Returns the raw platform signals and the rule that produced the classification. Reach for this first when stage detection returns something unexpected — it tells you whether the signal was absent, or present but carrying a realm token this version does not recognize.

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `Diagnostics` | `StageDiagnostics` | Raw signals, extracted realm, and the reason behind the classification |

#### `GetTraceContext`
Returns the underlying AWS trace and log context, for correlating ODC logs with infrastructure traces in tools like Datadog or New Relic.

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `Trace` | `TraceContext` | X-Ray trace identifier and CloudWatch log group and stream |

#### `GetRuntimeLifecycle`
Returns facts about the lifetime of the runtime process serving this library.

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `Lifecycle` | `RuntimeLifecycle` | Process uptime and whether this is the first call within the process |

#### `GetStageId`
Returns the unique identifier of the current stage.

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `StageId` | `Text` | Unique identifier of the current stage |

#### `GetRuntimeUrl`
Returns the URL the current stage is served from.

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `RuntimeUrl` | `Text` | URL the current stage is served from |

#### `GetRuntimeDetails`
Returns technical details about the server runtime.

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `Runtime` | `RuntimeDetails` | .NET version, OS, CPU, region, and serverless function info |

---

## Data Structures

### `StageDetails`
Classification and context for the current stage.
- `Classification`: Text — `Production`, `NonProduction`, `Development`, or `Unknown`
- `IsProduction`: Boolean
- `IsClassified`: Boolean — **`False` means the stage is genuinely unknown.** Never read `Unknown` as evidence of non-production
- `RuntimeUrl`: Text
- `Subdomain`: Text — a customer-controlled naming convention; **never use it for a security decision**
- `InfrastructureRealm`: Text
- `StageId`: Text

### `StageDiagnostics`
Raw signals behind the classification, for troubleshooting.
- `SecureGatewaySignal`: Text
- `ExtractedRealm`: Text
- `RealmRecognized`: Boolean
- `Classification`: Text
- `Reason`: Text — `SignalAbsent`, `UnrecognizedRealm`, or `RecognizedRealm`
- `RuntimeUrlSignal`: Text
- `ResolvedHost`: Text
- `KnownRealms`: Text — realms this version recognizes; if `ExtractedRealm` is absent from this list, the platform contract has changed

### `TraceContext`
AWS trace and log context.
- `XRayTraceId`: Text — **changes on every invocation; never cache it**
- `LogGroupName`: Text
- `LogStreamName`: Text

### `RuntimeLifecycle`
Lifetime facts for the runtime process.
- `UptimeMs`: Long Integer — milliseconds since the library was first loaded in this process
- `IsFirstCallInProcess`: Boolean — **not the same as the app's cold start**; the library may first be called long after the worker started

### `RuntimeDetails`
Technical details about the runtime that executes the external logic.
- `DotNetVersion`: Text
- `OperatingSystem`: Text
- `MachineName`: Text
- `ProcessorCount`: Integer
- `Is64BitOS`: Boolean
- `Is64BitProcess`: Boolean
- `AwsRegion`: Text
- `LambdaFunctionName`: Text
- `LambdaMemoryMB`: Integer

---

## Project Structure

```
RuntimeContext/
├── RuntimeContext.sln        # Solution file
├── RuntimeContext.csproj     # Project definition
├── IRuntimeContext.cs        # OSInterface & OSAction definitions
├── RuntimeContext.cs         # Environment reads; delegates all logic to Internal/
├── Internal/                 # Pure logic — no environment access, unit tested directly
│   ├── StageClassifier.cs    # Realm extraction and tier classification
│   └── HostParser.cs         # URL host and subdomain extraction
├── Resources/                # Branding assets
│   ├── app_icon.png          # Library icon
│   └── action_icon.png       # Action-level icon
├── Structures/               # ODC-compatible structs
│   ├── StageDetails.cs       # Stage classification and context
│   ├── StageDiagnostics.cs   # Raw signals and classification reason
│   ├── TraceContext.cs       # AWS trace and log context
│   ├── RuntimeLifecycle.cs   # Process uptime and first-call flag
│   └── RuntimeDetails.cs     # Runtime details
└── tests/
    └── RuntimeContext.Tests/ # xUnit test suite
```

The classification and parsing logic lives in `Internal/` as **pure functions** taking plain strings.
That keeps it unit-testable without mocks, interfaces, or a DI container — none of which ODC's
External Logic runtime provides, since it instantiates the `[OSInterface]` implementation itself
through a parameterless constructor.

**Run the tests:**
```bash
dotnet test
```

---

## Build and Deployment

1. **Publish:** Run `dotnet publish` as shown in Quick Start.
2. **Clean:** Delete `OutSystems.ExternalLibraries.SDK.dll` from the `publish/` directory.
3. **Zip:** Compress the remaining files into a flat structure (no subfolders).
4. **Deploy:** Upload to ODC Portal > External Logic.

---

## Verified Platform Signals

Observed on a live ODC tenant across all four stages (region `eu-central-1`, verified 2026-09-22):

| ODC stage | Stage purpose | Realm | `Classification` | `IsProduction` |
|---|---|---|---|---|
| Development | Development | `rundev` | `Development` | `False` |
| Testing | NonProduction | `runnp` | `NonProduction` | `False` |
| Pre-Production | NonProduction | **`runnp`** | `NonProduction` | `False` |
| Production | Production | `runp` | `Production` | **`True`** |

The raw signal observed behind the `Realm` column has the shape:

```
secure-gateway-sd-service-<environment-key>.<realm>.econnectivity.local
```

The realm is the second dot-label. The environment key embedded in the first label contains
hyphens but no dots, so it cannot shift that index.

Three facts this establishes:

1. **Testing and Pre-Production are indistinguishable** — both report `runnp`. Three tiers is the maximum resolution the signal supports.
2. **`Development` is genuinely distinct**, which is why it is reported as its own tier rather than collapsed into `NonProduction`.
3. **`StageId` equals the ODC environment key exactly**, so it is a reliable identifier for a specific stage even when the tier is ambiguous.

Every stage ran **.NET 10.0.11** on Amazon Linux 2023. External Logic executes in its own dedicated
Lambda (function names are prefixed `externallibrary-`), **separate from the app's own runtime** —
which is why `GetRuntimeLifecycle` reports this library's process lifetime and makes no claim about
the app's cold starts.

`GetTraceContext` was confirmed to return fully populated values in ODC — an X-Ray trace id, and a
CloudWatch log group and stream. Across two consecutive invocations the **trace id changed**
(`Root=1-6ab24495-…` → `Root=1-6ab244bd-…`) while `UptimeMs` advanced from `0` to `38928` on the
same worker. This is the observed behavior the no-caching rule exists for: caching that value would
have pinned every correlation for the worker's lifetime to the first request.

ODC may serve concurrent requests from **several workers**, so `IsFirstCallInProcess` and `UptimeMs`
describe whichever worker answered that call — not a single global process.

> These are observations of an undocumented contract at a point in time, not a guarantee. Use
> `ExplainClassification` to re-check after any ODC platform update.

---

## Notes and Best Practices

- **Production gating:** use `IsProductionStage` to guard production-only behavior. It fails closed — an `Unknown` result is never reported as Production. Read the directional warning at the top before inverting the check.
- **Unknown is not a stage.** `Unknown` means *no answer*, not *non-production*. Branch on `IsClassified` whenever the difference has consequences.
- **Unrecognized realms fail closed.** If ODC introduces a tier this version does not know, the result is `Unknown` rather than a confident wrong answer. Call `ExplainClassification` to see the unrecognized token and open an issue with it.
- **Three tiers is the honest maximum.** `Test` and `Pre-Production` share the **same** realm token (`runnp`) and are indistinguishable from the infrastructure signal — see the matrix below. Any library claiming to tell them apart is guessing. If you need that distinction, use the ODC stage identifier (`StageId`) or a per-stage app setting.
- **Undocumented signal:** stage detection reads an internal platform value that is verified against current ODC infrastructure but is not part of a documented contract — it may change on a platform update. For irreversible, production-only operations, consider also gating on a per-stage app setting.
- **Not a security boundary.** These signals are environment values in a container, not authenticated claims. Use them for operational convenience — logging, banners, feature flags — never as the sole control for anything security- or money-critical.
- **Secure Gateway dependency:** classification derives from a signal tied to an optional ODC feature. If your stages return `Unknown`, run `ExplainClassification` — the signal is likely absent rather than misread.

---

## Contributing

Contributions, bug reports, and feature requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on development setup, code conventions, and how to submit a pull request.

---

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
