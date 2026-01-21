# HealthData-Interoperability-Csharp

Professional implementation of **HL7 FHIR** standards using **C#** and **.NET 8**. This repository acts as a progressive portfolio, demonstrating end-to-end healthcare data interoperability—from foundational RESTful CRUD to advanced architectural patterns.

[![.NET](https://img.shields.io/badge/.NET-8.0-512bd4)](https://dotnet.microsoft.com/)
[![FHIR](https://img.shields.io/badge/FHIR-R4-flame.svg)](https://hl7.org/fhir/R4/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## 📌 Project Overview

In the modern healthcare landscape, data interoperability is the backbone of patient-centered care. This project showcases my technical proficiency in the **Fire.ly SDK**, building robust applications that speak the universal language of health data.

It is structured as a **multi-project solution**, where each module addresses a specific real-world interoperability challenge.



---

## 📂 Solution Roadmap (Modules)

| Module | Level | Focus | Status |
| :--- | :--- | :--- | :--- |
| **[01-Basic-FHIR-Client](./src/01-Basic-FHIR-Client)** | 🟢 Beginner | Foundation: SDK Setup, Patient Search, CRUD operations. | **Completed** |
| **[02-Advanced-Query](./src/02-Advanced-Query)** | 🟡 Intermediate | Complex Search: Chained params, `_include`, `_revinclude`. | *Planned* |
| **[03-Resource-Validator](./src/03-Resource-Validator)** | 🟡 Intermediate | Data Quality: Validation against Profiles (US Core / IG). | *Planned* |
| **[04-Data-Mapping-ETL](./src/04-Data-Mapping)** | 🔴 Advanced | Integration: Converting legacy CSV/JSON to FHIR Bundles. | *Planned* |
| **[05-SMART-on-FHIR](./src/05-SMART-on-FHIR)** | 🔴 Advanced | Security: OAuth2 Auth & Launch Context. | *Planned* |

---

## 🚀 Key Technical Features

### 1. Foundation (Phase 1)
* **FHIR Client Orchestration**: Production-ready `FhirClient` configuration with custom timeouts and headers.
* **Resource Handling**: Deep dive into the `Patient` resource model and JSON/XML serialization.
* **Safe Parsing**: Handling polymorphic FHIR types and terminology with the Fire.ly SDK.

### 2. Engineering Best Practices
* **Asynchronous Patterns**: Extensive use of `async/await` for non-blocking I/O.
* **Robust Error Handling**: Decoding `OperationOutcome` to provide meaningful feedback from the FHIR server.
* **Configuration-Driven**: Environment-based settings for FHIR Server endpoints (HAPI, Fire.ly, Azure).

---

## 🛠 Tech Stack

* **Language**: C# 12 / .NET 8
* **Core SDK**: [Fire.ly SDK (Hl7.Fhir.R4)](https://github.com/FirelyTeam/firely-net-sdk)
* **Testing**: xUnit (Planned for future modules)
* **Ecosystem**: Public FHIR Test Servers (HAPI / Fire.ly)

---

## 📖 Getting Started

### Prerequisites
* .NET 8.0 SDK
* A FHIR Test Server (Default: `https://server.fire.ly/r4`)

### Setup
1. **Clone the Repository**
   ```bash
   git clone [https://github.com/YourUsername/HealthData-Interoperability-Csharp.git](https://github.com/YourUsername/HealthData-Interoperability-Csharp.git)
   cd HealthData-Interoperability-Csharp