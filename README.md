# HealthData-Interoperability-Csharp

Professional implementation of **HL7 FHIR** standards using **C#** and **.NET 10**. This repository acts as a progressive portfolio, demonstrating end-to-end healthcare data interoperability—from foundational RESTful CRUD to advanced architectural patterns.

[![.NET](https://img.shields.io/badge/.NET-10.0-512bd4)](https://dotnet.microsoft.com/)
[![FHIR](https://img.shields.io/badge/FHIR-R4-flame.svg)](https://hl7.org/fhir/R4/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## 📌 Project Overview

In the modern healthcare landscape, data interoperability is critical. This project serves as a comprehensive showcase of my expertise in building FHIR-compliant applications using the **Fire.ly SDK**, ranging from basic RESTful interactions to advanced resource validation and complex search patterns.

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

## 🚀 Learning Path & Features

### Phase 1: The Foundation (Completed)
Based on the foundational principles of the [Fire.ly SDK](https://fire.ly/), I have implemented:
* **FHIR Client Setup**: Configuring the `FhirClient` with appropriate headers and settings.
* **Basic CRUD Operations**: Reading, creating, and searching for `Patient` resources.
* **Data Parsing**: Handling FHIR-specific JSON/XML serialization.
* **Error Handling**: Managing `OperationOutcome` and HTTP status codes in a healthcare context.

**Execution Result:**
<p align="left">
  <img src="https://github.com/memoryfraction/HealthData-Interoperability-Csharp/blob/main/images/01-Basic-FHIR-Client-printscreen.jpg" alt="FHIR Client Execution Output" width="750">
  <br>
  <em>Figure 1: Console output demonstrating Patient creation and search on .NET 10.0</em>
</p>

### Phase 2: Advanced Interoperability (In Progress)
* [ ] **Complex Search**: Implementing chained parameters and `_include`/`_revinclude`.
* [ ] **Resource Profiling**: Validating resources against specific StructureDefinitions.
* [ ] **Bundle Management**: Handling large datasets using FHIR Bundles and pagination.

---

## 🛠 Tech Stack

* **Language**: C# 12 / .NET 10 (LTS)
* **FHIR SDK**: [Fire.ly SDK (Hl7.Fhir.R4)](https://github.com/FirelyTeam/firely-net-sdk)
* **Tools**: Postman, Public Test Servers (HAPI FHIR / Fire.ly Server)

---

## 📖 Getting Started

### Prerequisites
* .NET 10.0 SDK or later
* An IDE (Visual Studio 2022, VS Code, or JetBrains Rider)

### Installation & Run
1. **Clone the Repository**
   ```bash
   git clone [https://github.com/memoryfraction/HealthData-Interoperability-Csharp.git](https://github.com/memoryfraction/HealthData-Interoperability-Csharp.git)
   cd HealthData-Interoperability-Csharp
