# Changelog

All notable changes to this project are documented here.
This project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.1.0]

### ⚠️ Behavior changes

These change the value of `StageDetails.Classification` in cases where it was previously
misleading. Review any logic that compares `Classification` to a literal string.

- **`Unknown` is no longer reported for unrecognized realms as `NonProduction`.** A realm token
  this version does not know now classifies as `Unknown` instead of `NonProduction`. Previously a
  future ODC tier would have been silently reported as a stage the library understood.
- **Development stages now classify as `Development`, not `NonProduction`.** The underlying signal
  always distinguished the development realm; the classifier was collapsing it. `IsProduction`
  is unchanged (still `False`) for these stages.

### Added

- `StageDetails.IsClassified` — `True` only when the stage was positively identified. This makes
  the previously invisible difference between *"confirmed non-production"* and *"no idea"*
  explicit. `IsProductionStage()` is fail-closed and returns `False` for both.
- `ExplainClassification()` — returns the raw platform signals, the extracted realm, the rule that
  fired, and the realms this version recognizes. Lets users self-diagnose when the undocumented
  platform contract changes, instead of silently misclassifying.
- `GetTraceContext()` — exposes `_X_AMZN_TRACE_ID`, `AWS_LAMBDA_LOG_GROUP_NAME`, and
  `AWS_LAMBDA_LOG_STREAM_NAME` for correlating ODC logs with infrastructure traces.
- `GetRuntimeLifecycle()` — process uptime and a first-call-in-process flag.
- Test suite (`tests/RuntimeContext.Tests`) — 37 tests covering classification, URL parsing,
  fail-closed behavior, and the SDK no-throw contract.

### Fixed

- **URL host parsing.** Subdomain extraction now normalizes the scheme before parsing with
  `System.Uri`, correctly handling ports, paths, query strings, and IPv6 literals. A bare
  host with a port (`acme.outsystems.app:443`) previously required hand-rolled string indexing;
  parsing it with `Uri` directly returns success with an **empty** host, treating the host as a
  URI scheme. Covered by regression tests.

### Notes

- Environment values remain **deliberately uncached**. `_X_AMZN_TRACE_ID` is rewritten by the
  Lambda runtime on every invocation, so caching it would pin every trace correlation for the
  life of the worker to the first request. A regression test guards this.

---

## [1.0.1]

### Changed

- Upgraded target framework to .NET 10 (was .NET 8).

---

## [1.0.0]

- Initial release: `GetCurrentStage`, `IsProductionStage`, `GetStageId`, `GetRuntimeUrl`,
  and `GetRuntimeDetails`.
