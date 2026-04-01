# RBAC & HIPAA Compliance Permission Design
## 07-HIPAA-Compliance-Demo

---

## 1. Overview (HIPAA Minimum Necessary Principle)
This document defines role-based access control (RBAC) for a FHIR-based healthcare application.
All designs strictly comply with HIPAA requirements:
- Least privilege access control
- Independent patient data isolation
- Field-level sensitive PHI redaction
- Full audit trace for all access behavior
- No unlimited super administrator account
- Permission enforced by BFF layer + Azure AD JWT authentication

---

## 2. Role Permission Matrix

### 2.1 SysAdmin
**Audience**: Clinic IT Manager
**Core Permissions**:
- GET: All system resources
- POST / PUT / DELETE: Only temporary emergency approval allowed
- View full audit logs

**Data Scope Restriction**:
Only system configuration and audit logs. Direct PHI access is prohibited.

**Implementation**:
1. Azure AD assigns built-in role `SysAdmin`
2. JWT claim contains `scope=system.admin`
3. BFF layer only permits access to AuditEvent and CapabilityStatement

---

### 2.2 Physician
**Audience**: Clinic attending physician
**Core Permissions**:
- GET / POST / PUT: Patient, Observation, Medication, Encounter
- DELETE operation is permanently forbidden

**Data Scope Restriction**:
Only patients linked via `Patient.generalPractitioner`.

**Implementation**:
1. JWT claims: `role=physician`, `doctorId`
2. BFF filters dataset by doctor identity
3. Block access to unrelated sensitive FHIR resources

---

### 2.3 Nurse
**Audience**: Clinic nursing staff
**Core Permissions**:
- GET: Patient, Observation, VitalSigns
- POST: VitalSigns only
- PUT / DELETE: Not allowed

**Data Scope Restriction**:
Only patients inside the current care team via `Patient.careTeam`.

**Implementation**:
1. JWT claims: `role=nurse`, `nurseId`
2. BFF automatically hides prescription and diagnosis sensitive fields

---

### 2.4 FrontDesk
**Audience**: Clinic front desk staff
**Core Permissions**:
- GET: Patient basic demographic + Appointment records
- All write and modify operations are forbidden

**Data Scope Restriction**:
Only name, contact, address and appointment data. All clinical PHI is hidden.

**Implementation**:
1. JWT claim: `role=frontdesk`
2. BFF performs field-level filtering to remove medical content

---

### 2.5 Biller
**Audience**: Insurance billing staff
**Core Permissions**:
- GET: Patient, Encounter, Claim, Procedure
- Modify or delete any PHI is forbidden

**Data Scope Restriction**:
Only billing necessary fields. Clinical result details are hidden.

**Implementation**:
1. JWT claim: `role=biller`
2. BFF masks all internal clinical observation values

---

### 2.6 Insurance External User
**Audience**: Partner insurance organization system account
**Core Permissions**:
- GET: Patient basic info, Encounter, Claim
- All write operations forbidden

**Data Scope Restriction**:
Only patients covered by the corresponding insurance plan.

**Implementation**:
1. Independent Azure AD service account with tenant claim
2. BFF filters records by `Patient.coverage` matching rule

---

### 2.7 Patient Self Portal
**Audience**: Patient account owner
**Core Permissions**:
- GET: All personal FHIR records
- PUT: Update personal contact and address info
- DELETE forbidden permanently

**Data Scope Restriction**:
Strict isolation: only access own patient ID records.

**Implementation**:
1. JWT claim contains `patientId`
2. BFF enforces identity matching on all query responses

---

### 2.8 HIPAA Auditor
**Audience**: Compliance audit reviewer
**Core Permissions**:
- GET: AuditEvent logs and patient metadata only
- No modification or full PHI viewing allowed

**Data Scope Restriction**:
Audit trail only, sensitive clinical content is redacted.

**Implementation**:
1. JWT claim: `role=auditor`
2. BFF restricts resource access to AuditEvent only

---

## 3. HIPAA Forbidden Rules
- No global super administrator role with full PHI access
- No bulk export or bulk delete permission for any user
- Cross-patient data access is strictly prohibited
- All batch operations limited to maximum 10 records with audit logging required

---

## 4. Security Architecture Flow
Azure AD Authentication -> JWT Claims -> BFF Permission Filter -> FHIR Backend API
JWT claims include role, userId, patientId, doctorId, nurseId and tenant information.
All permission checks and data filtering are centralized inside the BFF layer.

---

## 5. C# Role Enum Code
```csharp
public enum UserRole
{
    Admin,
    Physician,
    Nurse,
    FrontDesk,
    Biller,
    Insurance,
    Patient,
    Auditor
}
```

## 6. Verification Screenshot Checklist
SysAdmin cannot view real PHI content
Physician data access strictly limited to assigned patients
Nurse cannot view prescription sensitive records
FrontDesk only sees non-clinic basic information
Patient can only access personal records
Auditor only views audit logs without full PHI exposure
No super admin permission exists in the whole system