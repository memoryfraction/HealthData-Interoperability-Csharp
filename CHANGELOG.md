# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.4.3] - 2026-09-21

### Changed
- **NuGet package metadata** for all 4 packages (no code changes):
  - Added a `<Title>` to each package ("HealthData.Interop – …").
  - Descriptions now lead with the domain ("HIPAA-aware … for FHIR / healthcare
    interoperability apps") so the sub-packages are recognizable in nuget.org search,
    and point readers to `HealthData.Interop.Fhir` as the package most users want.
  - Wording standardized on "HIPAA-aware"; each description ends with
    "Provided as-is; does not by itself ensure HIPAA compliance."
  - Sub-package tags now include `fhir;hl7;healthcare;interoperability;hipaa;phi`.
- Versions: `HealthData.Interop.Fhir` **1.4.3**; `HealthData.Interop.Abstractions`,
  `HealthData.Interop.Logging.Extensions`, `HealthData.Interop.Logging.Serilog` **1.0.2**.

## [1.4.2] - 2026-09-21

### Fixed
- **NuGet Trusted Publishing**: the publish workflow now exchanges the GitHub OIDC
  token for a short-lived nuget.org API key via the official `NuGet/login@v1` action,
  then pushes with `dotnet nuget push`. The previous approach tried to use the raw
  GitHub OIDC JWT directly as a Bearer token against the push endpoint, which
  nuget.org rejected with `HTTP 403 AuthenticationFailed`.
- Meta-package `HealthData.Interop.Fhir` bumped to **1.4.2** (first successful
  keyless publish of the 4-package layout).

## [1.4.0] - 2026-09-21

### Added
- **Package split**: the single `HealthData.Interop.Fhir` package is now layered into 4 packages:
  - `HealthData.Interop.Abstractions` v1.0.0 — `IApplicationLogger`, `PhiMasker`, `SafeConsole`, `Guard`. **Zero third-party dependencies** (.NET BCL only).
  - `HealthData.Interop.Logging.Extensions` v1.0.0 — `IApplicationLoggerBridge` + `ToIApplicationLogger()` extension. Bridges `IApplicationLogger` to `Microsoft.Extensions.Logging.ILogger` (NLog, Serilog, or any provider) with automatic PHI masking.
  - `HealthData.Interop.Logging.Serilog` v1.0.0 — Serilog-backed `ConsoleLogger` with PHI masking. Depends on Abstractions + Serilog + Sinks.Console + Sinks.File.
  - `HealthData.Interop.Fhir` v1.4.0 — **meta-package** bundling all FHIR services. Depends on Abstractions + Logging.Extensions + Logging.Serilog + Hl7.Fhir.R4 + Firely.Fhir.Validation.R4 + CsvHelper + Duende.IdentityModel + Riok.Mapperly.
- **New solution file** `HealthData.Interop.slnx` (all 13 projects).
- **New `publish-packages.ps1`** — one-command build + test + pack + push all 4 packages to nuget.org.
- **New bridge tests** for `IApplicationLoggerBridge` (Information/Warning/Error/Critical mapping + PHI masking verification).

### Changed
- **Multi-targeting**: all library packages now target `net8.0` + `net10.0` (demos and tests remain `net10.0`).
- **IdentityModel 7.0.0 → Duende.IdentityModel 8.1.0** — package rename + namespace change (`IdentityModel.Client` → `Duende.IdentityModel.Client`). `OidcOptions` public API unchanged.
- **Hl7.Fhir.R4 6.0.2 → 6.5.0**, **Firely.Fhir.Validation.R4 3.1.0 → 3.3.1**, **Riok.Mapperly 4.1.1 → 4.3.1**, **Serilog 4.3.0 → 4.4.0**, **Serilog.Sinks.Console 6.0.0 → 6.1.1**, **Serilog.Sinks.File 6.0.0 → 7.0.0**.
- **Type forwarding** for source compatibility: `HealthDataInteropSharedLibrary.Shared.IApplicationLogger`, `.Guard`, `.PhiMasker`, `.SafeConsole`, and `.ConsoleLogger` are all type-forwarded to their new locations. Existing code using the old FQN still compiles without changes.
- Serilog, Serilog.Sinks.Console, Serilog.Sinks.File **removed from the meta-package** direct dependencies (now in `HealthData.Interop.Logging.Serilog`).
- `Microsoft.Extensions.Configuration.Json` **removed** (zero usage in library code; demo 05 uses it directly).

### Removed
- **Polly 8.4.2** — zero usage across the entire codebase. Dead dependency eliminated.

### Backward compatibility
- **Source-compatible**: all existing `using HealthDataInteropSharedLibrary.Shared;` + `new ConsoleLogger()` code compiles unchanged (type forwarding).
- **Binary-level change**: old binaries must be recompiled against the new assemblies. This is acceptable for a 1.x minor bump.

 - 2026-09-07

### Added
- **New module 08 — Data Drift Detection** (src/08-Data-Drift-Detector): read-only reconciliation tool that compares the legacy source-of-truth CSV against the FHIR resources synced by module 04, reporting per-record drift (missing / field-level drift / in sync).
- **New DriftDetectionService** in the shared library (HealthDataInteropSharedLibrary.DriftDetection namespace): Compare() for single-record comparison and DriftReport.Summarize() for batch reporting — purely additive public API, no existing types or method signatures changed.
- **README Scope & Stability** now states explicitly which FHIR architectural model (facade / hybrid / FHIR-native) this toolkit assumes, and where each module fits.
- **Test suite expanded** from 169 to **180 passing tests** (11 new in DriftDetectionServiceTests.cs), covering field-drift detection, missing-record detection, test-name-marker stripping, and a gender-comparison regression guard.

### Changed
- **PATCH bump (1.3.4 → 1.3.5),** backward-compatible and purely additive: a small, self-contained new demo module and shared-library namespace rather than a change to the library’s core surface.

## [1.3.4] - 2026-09-02

### Added
- **PHI-masked application logging.** `ConsoleLogger` (the Serilog-backed `IApplicationLogger`) now routes every message through `PhiMasker.Mask` before it reaches the sink, so SSN / patient name / DOB / phone / email are redacted instead of being written raw to audit logs.
- **`DevTlsBypass` helper.** Centralizes the `HEALTHDATA_INSECURE_SKIP_TLS` opt-in check used by all FHIR client constructors and demo modules, replacing four separate copies of the same environment-variable check.

### Changed
- **TLS certificate validation is now STRICT by default.** The former `enableHttps` constructor flag on `FhirBasicService` and `AdvancedQueryService` was removed; certificate validation is always enforced unless you explicitly opt in.
- **DEV-only TLS bypass is now opt-in via environment variable.** A certificate-validation bypass is available only for local development and is **OFF by default**. Enable it (dev only) by setting `HEALTHDATA_INSECURE_SKIP_TLS=1` before starting the process. Demo modules (`01-Basic-FHIR-Client`, `02-Advanced-Query`, `04-Data-Mapping-ETL`, `05-SMART-on-FHIR`) previously unconditionally bypassed certificate validation; they now require the explicit opt-in. Do **not** enable this in production (violates HIPAA §164.312(e)(1)).

### Fixed
- **README "Quick Start" example now compiles.** The previous snippet referenced a non-existent `HipaaComplianceOrchestrator.EvaluateAccess(...)` API; it now uses the real public API (`FhirBasicService`, `ResourceValidationService`).
- README Security Notice updated to reflect the gated (opt-in) TLS bypass and the PHI-masked logging.
- **Dev TLS bypass was a no-op in `FhirBasicService` and `AdvancedQueryService`.** Their opt-in callback checked `sslPolicyErrors == SslPolicyErrors.None`, which is identical to default validation and never actually bypassed a bad certificate — the documented "connect through a self-signed MITM proxy" workflow silently didn't work. Now consistent with the other two call sites.
- Removed duplicate/stale TLS security-notice comment blocks left behind in `04-Data-Mapping-ETL/Program.cs` and `05-SMART-on-FHIR/Program.cs`.
- `PhiMasker.SafeInformation/SafeWarning/SafeError/SafeCritical` no longer mask messages twice now that `ConsoleLogger` masks unconditionally.

## [1.3.3] - 2026-08-11
### Changed
- Code quality and stability improvements across the shared library.
### Removed
- **Polly dependency** removed to simplify the package surface area and reduce transitive dependencies.
### Fixed
- Minor issues resolved based on integration testing feedback.

## [1.3.2] - 2026-08-11
### Removed
- **Microsoft.SemanticKernel alpha dependency** removed — eliminated a known critical vulnerability warning (NU1904 / GHSA-2ww3-72rp-wpp4) and the NuGet NU5104 stable-release warning.
- Unnecessary transitive dependencies removed to reduce the NuGet package size.
### Changed
- Ollama integration switched from SemanticKernel to a lightweight native `HttpClient` (smaller package, zero alpha dependencies).

## [1.3.1] - 2026-08-11
### Added
- **FHIR R4 specification embedded** — `specification.zip` (~6MB) bundled via ContentFiles for reliable offline validation.
### Changed
- **`ResourceValidationService` rewritten** — removed ineffective Polly retry logic in favor of a clean init/fallback pattern.
- Module 04 now uses `HttpClientHandler` for consistent TLS behavior across demo modules.
### Fixed
- README images: replaced untrusted GitHub blob URLs with `raw.githubusercontent.com` CDN links.

## [1.3.0] - 2026-08-11
### Added
- **SMART on FHIR authentication** (`SmartOnFhirAuthService`) — OAuth2/OIDC Client Credentials flow with token caching, automatic refresh, and an authenticated `FhirClient` factory (HIPAA §164.312(a)(2)(iii)).
- **PHI encryption** (`PhiEncryptionService`) — AES-256-GCM (256-bit key, 12-byte nonce, 16-byte auth tag) for encryption at rest (HIPAA §164.312(a)(2)(iv)).
- **US Core conformance checking** (`UsCoreConformanceChecker`, `UsCoreProfiles`) — validates `Meta.Profile` URIs against US Core IG v7.1.0 for Patient, Observation, Encounter, Condition, MedicationRequest, AllergyIntolerance.
- **Logging abstraction** (`IApplicationLogger` + `ConsoleLogger`) — Serilog-backed, UTC-timestamped, for HIPAA audit-trail readiness.
### Changed
- Removed the duplicate `LegacyPatientRecord` DTO in Module 04; now uses the shared-library version.
- Test suite expanded from **163 → 169** passing tests (added coverage for encryption, auth, US Core conformance, and logging).

## [1.2.0] - 2026-08-09
### Added
- **Service-class extraction** across Modules 01–07 for reusable, unit-testable logic:
  - `FhirBasicService` (Module 01) — basic FHIR Patient CRUD.
  - `AdvancedQueryService` (Module 02) — chained search + `_include`/`_revinclude`.
  - `ResourceValidationService` (Module 03) — Firely FHIR resource validation.
  - `EtlPipelineService` (Module 04) — CSV extraction, transformation, transaction bundle loading.
  - `SmartFhirEtlService` (Module 05) — SMART-on-FHIR ETL with US Core profile support.
  - `AiValidatorService` + `ClinicalGuardrails` (Module 06) — AI-assisted data cleaning with guard validation.
  - `HipaaComplianceOrchestrator` (Module 07) — HIPAA compliance workflow orchestration.
- **Mapperly (Riok.Mapperly v4.1.1)** configured for compile-time mapping; centralized `GenderNormalizer`.
- `Guard` helper unified across the shared library (`NotNull`, `NotNullOrEmpty`).
### Fixed
- **Module 05 gender-mapping bug:** the old `record.Gender?.ToLower().Contains("male")` check matched the substring in `"female"` (fe-**male**) and imported female patients as `Male`. Centralized `GenderNormalizer` now maps `female`/`f`/`woman` → `Female` and unrecognized/empty values → `Unknown`.

## [1.1.0] - 2026-08-07
### Added
- **TDD unit-test suite** — MSTest v3 + FluentAssertions, **163 passing tests** across 6 test files (RBAC, consent, audit log, parameter validation, FHIR Patient model, `LegacyPatientRecord`).
- Bilingual (EN/CN) XML doc comments and parameter-validation guards on the shared library, per `docs/CodeStandard.md`.
### Changed
- Unified all project target frameworks to `net10.0` (the NuGet package still targets `net8.0` for broad compatibility).

<!-- Release links -->
[1.3.5]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
[1.3.4]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
[1.3.3]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
[1.3.2]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
[1.3.1]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
[1.3.0]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
[1.2.0]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
[1.1.0]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/

