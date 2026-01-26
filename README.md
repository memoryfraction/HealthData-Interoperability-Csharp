# HealthData-Interoperability-Csharp

Implementation of **HL7 FHIR** standards using **C#** and **.NET 10**. This repository acts as a progressive portfolio, demonstrating end-to-end healthcare data interoperability—from foundational RESTful CRUD to advanced architectural patterns.

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
| **[01-Basic-FHIR-Client](https://github.com/memoryfraction/HealthData-Interoperability-Csharp/tree/main/src/1-Basic-Client)** | 🟢 Beginner | Foundation: SDK Setup, Patient Search, CRUD operations. | **Completed** |
| **[02-Advanced-Query](https://github.com/memoryfraction/HealthData-Interoperability-Csharp/tree/main/src/02-Advanced-Query)** | 🟡 Intermediate | Complex Search: Chained params, `_include`, `_revinclude`. | **Completed** |
| **[03-Resource-Validator](https://github.com/memoryfraction/HealthData-Interoperability-Csharp/tree/main/src/03-Resource-Validator)** | 🟡 Intermediate | Data Quality: Validation against Profiles (US Core / IG). | **Completed** |
| **[04-Data-Mapping-ETL](https://github.com/memoryfraction/HealthData-Interoperability-Csharp/tree/main/src/04-Data-Mapping-ETL)** | 🔴 Advanced | Integration: Converting legacy CSV/JSON to FHIR Bundles. | **Completed** |
| **[05-SMART-on-FHIR](./src/05-SMART-on-FHIR)** | 🔴 Advanced | Security: OAuth2 Auth & Launch Context. | *Planned* |

---

## 🚀 Learning Path & Features

## Phase 1: The Foundation (Completed)
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

**Execution Result:**
<p align="left">
  <img src="https://github.com/memoryfraction/HealthData-Interoperability-Csharp/blob/main/images/02-Advanced-Query%20Result.jpg?raw=true" alt="Advanced Query Execution Output" width="750">
  <br>
  <em>Figure 2: Advanced search results showing Chained Params and Included resources.</em>
</p>
---

## Phase 03: FHIR Resource Validator (The Data Firewall)
### What does this code actually do?
In healthcare, "bad data" (like an invalid birth date) isn't just a bug—it's a clinical risk. This module implements a **Medical Data Firewall** using the Firely SDK.

Instead of writing hundreds of manual `if-else` statements, this code:
1. **Loads the "Official Law"**: It connects to `specification.zip` (the HL7 FHIR R4 standard definitions).
2. **Performs Semantic Audit**: It validates resources against complex medical rules, such as:
   - **Date Logic**: Identifying that `1990-13-45` is an impossible date.
   - **Data Integrity**: Checking if a `Telecom` entry is missing its required value.

### Why is this important?
- **Ensures Interoperability**: Guarantees that the data you produce will be accepted by other FHIR-compliant systems (like Epic or Cerner).
- **Automated Governance**: Automatically enforces cardinality (required fields) and value sets (allowed codes).

### Output Breakdown
If you see an **[ERROR]** in the console, **it means the code is working perfectly!** It successfully "caught" the invalid data before it could reach a database.

![Validation Result](https://github.com/memoryfraction/HealthData-Interoperability-Csharp/blob/main/images/phase03-validation-result.png.jpg)

## 04-Data-Mapping-ETL (Legacy Integration)
This module demonstrates a professional-grade ETL pipeline to migrate legacy CSV patient data into a FHIR R4 ecosystem.
* **Idempotency (幂等性)**: Implemented via **Conditional PUT**, ensuring that re-running the job updates existing records instead of creating duplicates.
* **Atomic Transactions**: Uses `Bundle.BundleType.Transaction` to guarantee data integrity.
* **Automated Verification**: The system automatically queries the server post-upload to confirm `VersionId` and `LastUpdated` metadata.

**Execution Result:**
<p align="left">
  <img src="https://github.com/memoryfraction/HealthData-Interoperability-Csharp/blob/main/images/04-Data-Mapping-ETL-result.jpg?raw=true" alt="ETL Result" width="750">
  <br>
  <em>Figure 4: ETL process showing successful load and automated verification.</em>
</p>


### Phase 2: Advanced Interoperability (In Progress)
* [ ] **Bundle Management**: Handling large datasets using FHIR Bundles and pagination.



## 🛠 Tech Stack
* **Language**: C# 12 / .NET 10 (LTS)
* **FHIR SDK**: [Fire.ly SDK (Hl7.Fhir.R4)](https://github.com/FirelyTeam/firely-net-sdk)
* **Tools**: Postman, Public Test Servers (HAPI FHIR / Fire.ly Server)

---

## 📚 References & Resources

To build and extend this project, the following resources were instrumental:

* **Foundational Tutorial**: [Make your first FHIR client within one hour](https://fire.ly/blog/make-your-first-fhir-client-within-one-hour/) - A primary guide for setting up the initial .NET FHIR client.
* **Fire.ly SDK Documentation**: [Official .NET SDK Docs](https://docs.fire.ly/projects/Firely-NET-SDK/) - Detailed guidance on serialization, search, and validation.
* **HL7 FHIR R4 Specification**: [FHIR Search Specs](https://www.hl7.org/fhir/search.html) - Understanding the logic behind chained parameters and includes.

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
