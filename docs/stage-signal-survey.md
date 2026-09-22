# Stage signal survey

Answers one question: **besides `SECURE_GATEWAY`, does ODC expose anything an External
Library can read to determine which stage it is running on?**

**No.** The survey below enumerates the complete environment of an ODC External Logic
runtime on a tenant where `SECURE_GATEWAY` is absent. No other variable carries tier
information. This is why [fallback-chain.md](fallback-chain.md) has only two states.

Surveyed 2026-09-22 on a personal ODC tenant, region `eu-west-1`, .NET 10.0.11 on
Amazon Linux 2023. External Logic runs in its own Lambda, `externallibrary-*`.

## Method

A temporary `DumpEnvironmentText` action enumerated `Environment.GetEnvironmentVariables()`
and additionally probed a list of candidate names by direct read, so that a name's
*absence* was recorded rather than silently omitted. Values were revealed only for keys
that cannot carry credentials; everything else reported name and length only.

Enumeration works in the ODC sandbox, so the listing is complete rather than a sample.
The probe has since been removed from the codebase.

## OutSystems-specific variables: the complete set

Four exist. None carries a stage tier.

| Variable | Example | Tier information |
|---|---|---|
| `OUTSYSTEMS_ENVIRONMENT_ID` | `c7262e9b-1f23-4e5f-a5d3-6f11e91ba6c2` | None — opaque UUID, equals the ODC environment key |
| `OUTSYSTEMS_TENANT_ID` | `3d7902d6-c8b3-4d46-8e10-594a24880cef` | None — identifies the organization |
| `OUTSYSTEMS_RUNTIME_URL` | `personal-3z8tb3n4-dev.outsystems.app` | Only through host naming, which is customer-controlled — see Rejected below |
| `OUTSYSTEMS_LOGGING_TAG_ID` | 80-character opaque token | None |

`SECURE_GATEWAY` was **absent**, which is why classification reports `Unknown` on this
tenant. On a tenant where Secure Gateway is provisioned it is shaped
`secure-gateway-sd-service-<environment-key>.<realm>.econnectivity.local`.

## Candidates checked and absent

Every plausible tier carrier was probed and does not exist:

`OUTSYSTEMS_ENVIRONMENT_NAME` · `OUTSYSTEMS_ENVIRONMENT_PURPOSE` ·
`OUTSYSTEMS_ENVIRONMENT_TYPE` · `OUTSYSTEMS_STAGE` · `OUTSYSTEMS_STAGE_NAME` ·
`OUTSYSTEMS_STAGE_TYPE` · `OUTSYSTEMS_APP_NAME` · `OUTSYSTEMS_APP_KEY` ·
`OUTSYSTEMS_APPLICATION_KEY` · `OUTSYSTEMS_MODULE_NAME` · `OUTSYSTEMS_ORGANIZATION` ·
`OUTSYSTEMS_PORTFOLIO_KEY` · `OUTSYSTEMS_REGION` · `OUTSYSTEMS_TENANT_NAME` ·
`ASPNETCORE_ENVIRONMENT` · `DOTNET_ENVIRONMENT` · `ENVIRONMENT` · `STAGE`

## Everything else

Stock AWS Lambda plumbing, none of it tier-related: `AWS_REGION`, `AWS_DEFAULT_REGION`,
`AWS_EXECUTION_ENV`, `AWS_LAMBDA_*` (function name, version, memory, log group, log
stream, runtime API, metadata API, initialization type), `AWS_ACCOUNT_ID`,
`AWS_XRAY_*`, `_X_AMZN_TRACE_ID`, plus OS-level entries (`PATH`, `LANG`, `TZ`, `PWD`,
`SHLVL`, `LD_LIBRARY_PATH`, `DOTNET_ROOT`, `LAMBDA_*`, `_HANDLER`).

Credentials are present in the environment — `AWS_ACCESS_KEY_ID`,
`AWS_SECRET_ACCESS_KEY`, `AWS_SESSION_TOKEN`, `AWS_LAMBDA_METADATA_TOKEN` — which is why
any future diagnostic must redact by default rather than dump.

## Rejected: inferring tier from the runtime URL

`OUTSYSTEMS_RUNTIME_URL` ends in `-dev` on this tenant, and MC's stages read `mc-dev`,
`mc-test`, `mc-test-1` and `mc`. That is a naming convention the customer controls, not a
platform contract. A library that concludes "production" from a substring will eventually
conclude it wrongly, and the failure mode is a false Production negative on a stage that
happens to be named differently. Not used, at any confidence level.

## Consequences

1. `SECURE_GATEWAY` is the only authoritative stage signal ODC exposes.
2. Tenants without Secure Gateway cannot be classified by the library alone, at all.
3. `OUTSYSTEMS_ENVIRONMENT_ID` is present and stable everywhere observed, so a
   caller-supplied environment-key-to-tier mapping is the only viable second source.
4. `OUTSYSTEMS_TENANT_ID` must not be exposed as a tenant identifier. It describes the
   deployment, not the current request's tenant, and would invite cross-tenant data leaks
   if used to scope queries.
