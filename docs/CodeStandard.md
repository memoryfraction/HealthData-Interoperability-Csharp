All code must adhere to SOLID and KISS principles.
All public methods and variables must include XML comments in both Chinese and English.
All public methods must begin with parameter validation checks.
All `Console.WriteLine()` calls, logs, and exception messages must be in English to avoid character encoding issues; using `UtilityService.LogAndWriteLine()` is encouraged whenever possible.
If a database is used, all timestamps must be in UTC.
All enumerations must be centrally located and managed.
For Blazor Client WASM applications, data processing can safely occur on the client side without transmission to the backend, ensuring HIPAA compliance.
Document the project scope, current code status, and overall architecture (in English), including version numbers, dates, and change logs; use standard Markdown formatting and keep the documentation updated alongside code iterations.
Readers should be able to quickly grasp the project's purpose, rationale, actual results, architecture, and data flow. When updating, read the current version and add new content rather than altering the formatting haphazardly.
Sensitive data (e.g., API keys, secrets) must not be uploaded to the GitHub repository.
Implement each feature in a separate branch and submit via Pull Request (PR) to ensure the `main` branch remains 100% functional. Example branch naming convention: `features/jwt_miniAuth`.
Do not modify code logic when implementing coding standards.
Follow TDD principles and prioritize testing; aim for high unit test coverage using MSTest.
