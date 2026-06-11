# Contributing to Runtime Context for ODC

Thank you for your interest in contributing. This document covers how to report issues, suggest improvements, and submit changes.

---

## Reporting Issues

Use [GitHub Issues](https://github.com/promonteiro89/odc-stage-context/issues) to report bugs or request features.

When reporting a bug, include:
- The version of the library you are using
- The ODC stage type where the issue occurs (Development, Test, Pre-Production, or Production)
- The action that produced the unexpected result
- What you expected vs. what you observed

---

## Development Setup

**Requirements:**
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- An ODC organization to test the built package

**Build:**
```bash
dotnet build RuntimeContext.csproj -c Release
```

**Publish for ODC:**
```bash
dotnet publish RuntimeContext.csproj -c Release -f net8.0 --no-self-contained
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

- Target **net8.0** — do not change the target framework without prior discussion.
- Keep all actions **input-free**. The library reads the environment directly; callers should never need to supply configuration.
- **Fails safe on the Production check:** if the infrastructure signal is absent or unrecognized, classify as Non-Production. Never return a false Production positive.
- All environment variable names are named constants — no inline string literals for env var names.
- Wrap every external call (`Environment.GetEnvironmentVariable`, `RuntimeInformation.*`, etc.) in the existing `Safe`/`Env` helpers to prevent the library from throwing across the SDK boundary.
- No comments that restate what the code does — only add one if it explains a non-obvious constraint or platform behavior.

---

## License

By contributing you agree that your contributions will be licensed under the [MIT License](LICENSE).
