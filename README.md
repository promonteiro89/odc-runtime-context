# Runtime Context for ODC

[![Platform](https://img.shields.io/badge/Platform-OutSystems_ODC-red.svg)](https://www.outsystems.com/odc/)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Dependencies](https://img.shields.io/badge/Dependencies-None-brightgreen.svg)](#)

A lightweight .NET 8.0 External Logic component for OutSystems Developer Cloud (ODC) that lets an app discover **which stage it is running on** — and in particular **whether it is Production** — at runtime, with zero configuration.

## Table of Contents

- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Quick Start](#quick-start)
- [Action Reference](#action-reference)
- [Data Structures](#data-structures)
- [Project Structure](#project-structure)
- [Build and Deployment](#build-and-deployment)
- [Notes and Best Practices](#notes-and-best-practices)
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
- **Stateless execution:** every action reads the environment on demand; there is no shared state, ensuring thread safety in high-concurrency ODC environments.
- **Fails safe:** anything not clearly identified as the Production plane is reported as non-production, so the component never returns a false positive for Production.
- **Resource embedding:** branded icons are embedded directly into the assembly for an integrated experience in ODC Studio.

---

## Prerequisites

- [OutSystems Developer Cloud (ODC)](https://www.outsystems.com/odc/)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

No third-party packages — the library depends only on the OutSystems External Libraries SDK (compile-time) and the .NET base class library.

---

## Quick Start

```bash
# Build
dotnet build RuntimeContext.csproj -c Release

# Publish for ODC
dotnet publish RuntimeContext.csproj -c Release -f net8.0 --no-self-contained
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
Quick boolean check, ideal for guarding production-only logic.

**Outputs:**
| Output | Type | Description |
|--------|------|-------------|
| `IsProduction` | `Boolean` | True when running on a Production stage |

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
- `Classification`: Text — `Production`, `NonProduction`, or `Unknown`
- `IsProduction`: Boolean
- `RuntimeUrl`: Text
- `Subdomain`: Text
- `InfrastructureRealm`: Text
- `StageId`: Text

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
├── RuntimeContext.sln       # Solution file
├── RuntimeContext.csproj    # Project definition
├── IRuntimeContext.cs       # OSInterface & OSAction definitions
├── RuntimeContext.cs        # Implementation & environment reads
├── Resources/               # Branding assets
│   ├── app_icon.png         # Library icon
│   └── action_icon.png      # Action-level icon
└── Structures/              # ODC-compatible structs
    ├── StageDetails.cs       # Stage classification and context
    └── RuntimeDetails.cs     # Runtime details
```

---

## Build and Deployment

1. **Publish:** Run `dotnet publish` as shown in Quick Start.
2. **Clean:** Delete `OutSystems.ExternalLibraries.SDK.dll` from the `publish/` directory.
3. **Zip:** Compress the remaining files into a flat structure (no subfolders).
4. **Deploy:** Upload to ODC Portal > External Logic.

---

## Notes and Best Practices

- **Production gating:** use `IsProductionStage` to guard production-only behavior. Because the classification fails safe, an `Unknown` result is treated as non-production.
- **Undocumented signal:** stage detection reads an internal platform value that is verified against current ODC infrastructure but is not part of a documented contract — it may change on a platform update. For irreversible, production-only operations, consider also gating on a per-stage app setting.

---

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
