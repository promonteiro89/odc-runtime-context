# Fallback chain — design for 1.2.0

Status: **specified, not implemented.** Supersedes the earlier three-tier draft.

## Problem

Stage classification derives entirely from `SECURE_GATEWAY`, which is tied to an optional
ODC feature. Where it is not provisioned, every stage classifies as `Unknown` and the
library's core promise does not hold. Observed on a personal tenant: correct, fail-closed,
and useless.

## What changed since the first draft

The draft assumed a middle "inferred" tier existed. The signal survey
([stage-signal-survey.md](stage-signal-survey.md)) enumerated the complete External Logic
environment and found **no second tier signal**. The only OutSystems variables that exist
are `OUTSYSTEMS_ENVIRONMENT_ID`, `OUTSYSTEMS_TENANT_ID`, `OUTSYSTEMS_RUNTIME_URL` and
`OUTSYSTEMS_LOGGING_TAG_ID`, none of which carries a tier.

So the chain has two sources, not three. There is nothing to infer from, and inventing an
inference tier would trade a useless answer for a wrong one.

## The chain

| # | Source | Confidence | Behaviour |
|---|---|---|---|
| 1 | `SECURE_GATEWAY` realm | `InfrastructureSignal` | Current logic, unchanged |
| 2 | Caller-supplied `StageId` mapping | `ConfiguredMapping` | Match `OUTSYSTEMS_ENVIRONMENT_ID` against the caller's list |
| 3 | — | `None` | `Unknown`, fail closed |

`OUTSYSTEMS_ENVIRONMENT_ID` is present and stable on every tenant observed and equals the
ODC environment key, which is what makes source 2 workable. The library cannot map that
UUID to a tier on its own; whoever owns the estate can.

## API surface

Additive. The existing eight actions are unchanged.

```
StageDetails.ClassificationSource     : InfrastructureSignal | ConfiguredMapping | None
StageDiagnostics.ClassificationSource : same
```

`IsClassified` keeps its meaning — true when any source positively identified the stage.
`ClassificationSource` says which one, so a caller can require authoritative-only for a
money-critical gate.

```
ClassifyStage(StageMapping[] mapping) -> StageDetails
StageMapping { StageId : Text, Classification : Text }
```

This is the only action that takes an input, and the exception is deliberate. CONTRIBUTING
requires actions to stay input-free; that rule assumes a signal exists to read. Where none
does, the honest alternatives are an input or a wrong answer.

## Behaviour matrix

| Signal | Mapping supplied | Classification | IsClassified | Source |
|---|---|---|---|---|
| `runp` | — | `Production` | True | `InfrastructureSignal` |
| absent | matches current StageId | caller's value | True | `ConfiguredMapping` |
| absent | supplied, no match | `Unknown` | False | `None` |
| absent | none | `Unknown` | False | `None` |
| unrecognized realm | any | `Unknown` | False | `None` |

The last row matters: an unrecognized realm **never** falls through to the mapping. A
signal that exists but is not understood means the platform contract changed, and that
must surface as `Unknown` rather than be papered over by configuration.

## Tests

- Every matrix row, as pure `StageClassifier` tests
- A mapping never overrides a present authoritative signal
- An unrecognized realm is not rescued by a mapping
- Empty, null and duplicate-key mappings degrade to `Unknown` and never throw
- `ClassificationSource` correct on every path

## Docs

- README gains the chain table, and states that `ConfiguredMapping` is exactly as
  trustworthy as the mapping supplied
- The directional `IsProductionStage()` warning gains a line: it returns True only on
  `InfrastructureSignal` or `ConfiguredMapping`, never on inference
- CHANGELOG: additive, no behaviour change for existing consumers

## Open question

Whether `ClassifyStage(mapping)` earns its place at all. `GetStageId()` already exists, and
a caller can map it in a few lines of their own logic. The library's honest value-add is
one branch instead of two, plus `ClassificationSource`. Worth deciding before building.

## Not doing

Inferring tier from the runtime URL host. The naming is customer-controlled, so the failure
mode is a confident wrong answer. See the survey's Rejected section.
