# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
[1.3.4]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
[1.3.3]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
[1.3.2]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
[1.3.1]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
[1.3.0]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
[1.2.0]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
[1.1.0]: https://github.com/memoryfraction/HealthData-Interoperability-Csharp/
