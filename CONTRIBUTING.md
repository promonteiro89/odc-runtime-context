# Contributing to Runtime Context for ODC

Thank you for your interest in contributing. This document covers how to report issues, suggest improvements, and submit changes.

---

## Reporting Issues

Use [GitHub Issues](https://github.com/promonteiro89/odc-runtime-context/issues) to report bugs or request features.

When reporting a bug, include:
- The version of the library you are using
- The ODC stage type where the issue occurs (Development, Test, Pre-Production, or Production)
- The action that produced the unexpected result
- What you expected vs. what you observed

---

## Development Setup

**Requirements:**
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- An ODC organization to test the built package

**Build:**
```bash
dotnet build RuntimeContext.csproj -c Release
```

**Test:**
```bash
dotnet test
```

**Publish for ODC:**
```bash
dotnet publish RuntimeContext.csproj -c Release -f net10.0 --no-self-contained
```

After publishing, zip the `publish/` output (excluding `OutSystems.ExternalLibraries.SDK.dll`) and upload it to your ODC Portal under **External Logic** to test your changes.

---

## Submitting Changes

1. Fork the repository and create a branch from `main`.
2. Make your changes. Keep each PR focused on a single concern.
3. Ensure the project builds cleanly with no warnings (`dotnet build -c Release`).
4. Open a Pull Request against `main` with a clear description of what changed and why.

---

## Code Conventions

- Target **net10.0** — do not change the target framework without prior discussion.
- Keep all actions **input-free**. The library reads the environment directly; callers should never need to supply configuration.
- **Fails closed on the Production check:** if the infrastructure signal is absent or unrecognized, classify as `Unknown`. Never return a false Production positive, and never report an unrecognized realm as a tier we claim to understand.
- **Never conflate `Unknown` with `NonProduction`.** They are different answers with different consequences: consumers invert the production check, and reporting "not production" when we mean "no idea" routes production traffic into test behavior. Keep `IsClassified` accurate.
- **Put logic in `Internal/` as pure functions** over plain strings, and keep `RuntimeContext` a thin environment-reading shell. ODC instantiates the `[OSInterface]` implementation via a parameterless constructor and provides no DI container, so constructor injection is not an option — purity is how this codebase stays testable.
- **`[OSStructure]` types must stay mutable** (public parameterless constructor plus `get; set;` properties). The SDK materializes them by setting properties; `readonly struct` and `readonly record struct` break that contract. Internal types should be immutable.
- **Do not cache environment reads.** `_X_AMZN_TRACE_ID` is rewritten by the Lambda runtime on every invocation — caching it silently pins every trace correlation for the worker's lifetime to the first request. A regression test guards this; do not weaken it. The per-call cost is nanoseconds against millisecond SDK marshalling overhead.
- All environment variable names are named constants — no inline string literals for env var names.
- **Every new action needs tests.** Pure logic goes in a dedicated test class; anything mutating the process environment belongs in `RuntimeContextEnvironmentTests` so xUnit serializes it.
- Wrap every external call (`Environment.GetEnvironmentVariable`, `RuntimeInformation.*`, etc.) in the existing `Safe`/`Env` helpers to prevent the library from throwing across the SDK boundary.
- No comments that restate what the code does — only add one if it explains a non-obvious constraint or platform behavior.

---

## License

By contributing you agree that your contributions will be licensed under the [MIT License](LICENSE).
