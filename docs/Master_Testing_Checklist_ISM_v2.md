# Master Testing Checklist

**Sponsor Management and Letter of Guarantee (LoG) Coverage Integration System**  
**International School Manila (ISM)**

## Instructions

- Use this checklist for pilot testing with prepared system accounts and scripted role-based validation.
- Record each item as **Pass**, **Pass with Conditions**, or **Fail** where applicable.
- Document all blocking issues in the **Defect and Retest Log**.
- Functional Testing includes a **Role** column to identify which account level should execute the test.

---

## A. Test Setup Checklist

| Item | Expected Condition | Status | Remarks |
|---|---|---|---|
| Pilot application URL is accessible | Application opens successfully |  |  |
| Stable build/version is identified | Version/build is recorded |  |  |
| Test date is recorded | Testing date is documented |  |  |
| Admin test account is ready | Login credentials work |  |  |
| Admissions test account is ready | Login credentials work |  |  |
| Cashier test account is ready | Login credentials work |  |  |
| Sponsor test account is ready | Login credentials work |  |  |
| Test sponsor records are available | Records can be retrieved |  |  |
| Test student records are available | Records can be retrieved |  |  |
| Test LoG records are available | Records can be retrieved |  |  |
| Duplicate sponsor scenario is prepared | Duplicate test case exists |  |  |
| Change request scenario is prepared | Request test case exists |  |  |
| Activation/deactivation scenario is prepared | Status-change test case exists |  |  |
| Swagger access is ready | Endpoints can be checked |  |  |
| Defect log sheet is ready | Defects can be recorded |  |  |

---

## B. Smoke Testing Checklist

| Test ID | Check | Expected Result | Status | Remarks |
|---|---|---|---|---|
| SMK-01 | Open pilot URL | Application loads successfully |  |  |
| SMK-02 | Login using Admin account | Dashboard loads |  |  |
| SMK-03 | Login using Admissions account | Dashboard loads |  |  |
| SMK-04 | Login using Cashier account | Dashboard loads |  |  |
| SMK-05 | Login using Sponsor account | Dashboard loads |  |  |
| SMK-06 | Open Sponsor Profile module | Module opens without error |  |  |
| SMK-07 | Open Letters of Guarantee module | Module opens without error |  |  |
| SMK-08 | View one sponsor record | Record opens correctly |  |  |
| SMK-09 | Perform one basic save or update | Action succeeds |  |  |
| SMK-10 | Logout | Session ends successfully |  |  |
| SMK-11 | Try restricted page with wrong role | Access is blocked |  |  |

---

## C. Functional Testing Checklist

| Test ID | Role | Check | Expected Result | Status | Remarks |
|---|---|---|---|---|---|
| FUN-01 | All Roles | Valid login | Access granted to correct role |  |  |
| FUN-02 | All Roles | Invalid login | Access denied |  |  |
| FUN-03 | Admin / Admissions | Missing required field | Save is blocked |  |  |
| FUN-04 | Admin / Admissions / Sponsor | Invalid field format | Validation message appears |  |  |
| FUN-05 | Admin / Admissions | Create sponsor record | Record is saved successfully |  |  |
| FUN-06 | Admin / Admissions / Cashier | Search sponsor by ID | Correct record appears |  |  |
| FUN-07 | Admin / Admissions / Cashier | Search sponsor by name | Correct record appears |  |  |
| FUN-08 | Admin / Admissions / Cashier / Sponsor | View sponsor record | Details display correctly |  |  |
| FUN-09 | Admin / Admissions | Edit sponsor record | Changes are saved successfully |  |  |
| FUN-10 | Admin / Admissions / Sponsor | View sponsor contacts | Contacts display correctly |  |  |
| FUN-11 | Admin | Detect duplicate sponsor | Duplicate candidate is identified |  |  |
| FUN-12 | Admin | Merge duplicate sponsor | One valid record remains and action is traceable |  |  |
| FUN-13 | Sponsor | Submit sponsor change request | Request is recorded successfully |  |  |
| FUN-14 | Admin | Apply sponsor change request | Sponsor record updates correctly |  |  |
| FUN-15 | Admin / Admissions / Cashier / Sponsor | View LoG list | LoG list loads correctly |  |  |
| FUN-16 | Admin / Admissions / Cashier / Sponsor | Open coverage view | Coverage details display correctly |  |  |
| FUN-17 | Admin | Deactivate LoG | Status changes correctly |  |  |
| FUN-18 | Admin | Reactivate LoG | Status changes correctly |  |  |
| FUN-19 | Admin | Retrieve audit record | Audit information is available |  |  |
| FUN-20 | Admin / Admissions / Cashier / Sponsor | Failed action handling | Standardized error behavior appears |  |  |

---

## D. Integration Testing Checklist

| Test ID | Check | Expected Result | Status | Remarks |
|---|---|---|---|---|
| INT-01 | Sponsor save and retrieval | Saved sponsor data is retrievable |  |  |
| INT-02 | Sponsor edit persistence | Updated data remains after refresh |  |  |
| INT-03 | Sponsor-student relationship | Linked records remain correct |  |  |
| INT-04 | School year and LoG relationship | Correct school-year linkage is preserved |  |  |
| INT-05 | LoG and sponsor linkage | Correct sponsor is shown in LoG record |  |  |
| INT-06 | Status propagation | Status appears correctly in list and detail views |  |  |
| INT-07 | Invalid transaction handling | No inconsistent data remains after failed action |  |  |
| INT-08 | Swagger endpoint check for retrieval | Response matches UI data |  |  |
| INT-09 | Swagger endpoint check for save/update | Backend result matches executed action |  |  |
| INT-10 | Swagger error response check | Invalid request returns expected error |  |  |

---

## E. Security Testing Checklist

| Test ID | Check | Expected Result | Status | Remarks |
|---|---|---|---|---|
| SEC-01 | Invalid password login | Access denied |  |  |
| SEC-02 | Sponsor tries to access Admin page | Access blocked |  |  |
| SEC-03 | Cashier tries to access Settings | Access blocked |  |  |
| SEC-04 | Admissions tries admin-only action | Action blocked |  |  |
| SEC-05 | Direct URL access to restricted page | Access blocked |  |  |
| SEC-06 | Logout then use back button | Protected page is not accessible |  |  |
| SEC-07 | Invalid form submission | Submission blocked |  |  |
| SEC-08 | Script-like input in text field | Input safely handled or blocked |  |  |
| SEC-09 | Search field with unusual input | No abnormal behavior occurs |  |  |
| SEC-10 | Important create/update action | Action is traceable in logs/audit |  |  |
| SEC-11 | Status change action | Action is traceable in logs/audit |  |  |

---

## F. Performance Observation Checklist

| Test ID | Check | Expected Result | Status | Remarks |
|---|---|---|---|---|
| PER-01 | Login response | Loads within acceptable time |  |  |
| PER-02 | Sponsor search response | Returns within acceptable time |  |  |
| PER-03 | Sponsor save response | Completes within acceptable time |  |  |
| PER-04 | Sponsor edit response | Completes within acceptable time |  |  |
| PER-05 | LoG list load response | Loads within acceptable time |  |  |
| PER-06 | Coverage view load response | Loads within acceptable time |  |  |

---

## G. Role-Based Workflow Validation Checklist

| Test ID | Check | Expected Result | Status | Remarks |
|---|---|---|---|---|
| RBV-ADM-01 | Login as Admin | Admin dashboard opens |  |  |
| RBV-ADM-02 | Access Sponsor Profile | Module is accessible |  |  |
| RBV-ADM-03 | Create sponsor | Record is saved |  |  |
| RBV-ADM-04 | Edit sponsor | Changes persist |  |  |
| RBV-ADM-05 | Deactivate/Reactivate LoG | Status changes correctly |  |  |
| RBV-ADM-06 | Review trace or audit record | Record is accessible |  |  |
| RBV-ADMS-01 | Login as Admissions | Dashboard opens |  |  |
| RBV-ADMS-02 | Search sponsor | Correct record appears |  |  |
| RBV-ADMS-03 | Add sponsor | Record saves correctly |  |  |
| RBV-ADMS-04 | Edit sponsor | Changes persist |  |  |
| RBV-ADMS-05 | Review LoG status | Correct status appears |  |  |
| RBV-ADMS-06 | Review pending requests | Requests are visible if applicable |  |  |
| RBV-CASH-01 | Login as Cashier | Dashboard opens |  |  |
| RBV-CASH-02 | Search sponsor | Correct sponsor info appears |  |  |
| RBV-CASH-03 | Review LoG list | LoG records are visible |  |  |
| RBV-CASH-04 | Open coverage view | Coverage details appear |  |  |
| RBV-CASH-05 | Attempt restricted settings access | Access blocked |  |  |
| RBV-SPON-01 | Login as Sponsor | Sponsor view opens |  |  |
| RBV-SPON-02 | View own sponsor record | Own record displays correctly |  |  |
| RBV-SPON-03 | View sponsor contacts | Contact details appear |  |  |
| RBV-SPON-04 | Submit change request | Request is recorded |  |  |
| RBV-SPON-05 | View sponsor-relevant status | Correct status is visible |  |  |
| RBV-SPON-06 | Attempt restricted admin action | Access blocked |  |  |

---

## H. Defect and Retest Checklist

| Defect ID | Related Test ID | Issue Summary | Severity | Fix Applied | Retest Result |
|---|---|---|---|---|---|
|  |  |  |  |  |  |
|  |  |  |  |  |  |
|  |  |  |  |  |  |
|  |  |  |  |  |  |
|  |  |  |  |  |  |
|  |  |  |  |  |  |
|  |  |  |  |  |  |
|  |  |  |  |  |  |

---

## I. Final Summary Checklist

| Item | Status | Remarks |
|---|---|---|
| Smoke testing completed |  |  |
| Functional testing completed |  |  |
| Integration testing completed |  |  |
| Security testing completed |  |  |
| Performance observation completed |  |  |
| Role-based workflow validation completed |  |  |
| All failed cases logged |  |  |
| Retests completed |  |  |
| Final test summary prepared |  |  |
| Manuscript results updated |  |  |
