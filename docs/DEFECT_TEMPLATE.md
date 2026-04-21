# Defect Report Template
## ISM Sponsor Management System UAT

---

## Defect Information

**Defect ID:** [Auto-generated or manual ID]  
**Test Case ID:**  [e.g., UT03, UT05]  
**Date Reported:** [MM/DD/YYYY]  
**Reported By:** [Tester Name]  
**Tester Role:** [Admin / Admissions / Cashier / Sponsor]

---

## Defect Summary

**Summary:** [One-line description of the issue]

**Severity:** ☐ Critical  ☐ High  ☐ Medium  ☐ Low

**Priority:** ☐ P1 (Immediate)  ☐ P2 (High)  ☐ P3 (Medium)  ☐ P4 (Low)

**Status:** ☐ New  ☐ Assigned  ☐ In Progress  ☐ Resolved  ☐ Closed  ☐ Deferred

---

## Defect Details

### Module/Feature Affected
[e.g., Sponsor Management, LoG Activation, Change Request Approval, Reports]

### Environment
- **URL:** [Staging/Test environment URL]
- **Browser:** [Chrome/Firefox/Edge + version]
- **OS:** [Windows/macOS/Linux]
- **User Account:** [demo.admin@ism.edu.ph, etc.]
- **School Year:** [2025-2026]

### Steps to Reproduce
1. [First step]
2. [Second step]
3. [Third step]
4. [Continue...]

### Expected Result
[What should happen according to requirements or test script]

### Actual Result
[What actually happened]

### Evidence
- **Screenshot(s):** [Attach or reference screenshot files]
- **Screen Recording:** [If available]
- **Browser Console Errors:** [Copy/paste any console errors]
- **Log Excerpt:** [If applicable]
- **Error Message Text:** [Exact text of any error messages]

---

## Impact Assessment

**User Impact:**  
[Describe how this affects end users - e.g., "Users cannot create sponsors", "Workaround available but confusing"]

**Business Impact:**  
[Describe operational impact - e.g., "Blocks pilot deployment" vs "Cosmetic issue only"]

**Frequency:**  
☐ Always (100%)  ☐ Often (>50%)  ☐ Sometimes (10-50%)  ☐ Rare (<10%)

---

## Additional Information

### Workaround
[If a workaround exists, describe it here]

**Workaround Acceptable for Pilot?** ☐ Yes  ☐ No

### Related Defects
[List any related defect IDs or dependencies]

### Notes
[Any additional context, observations, or suggestions]

---

## Resolution Tracking

**Assigned To:** [Developer name]  
**Assigned Date:** [MM/DD/YYYY]  
**Target Resolution Date:** [MM/DD/YYYY]

**Root Cause:**  
[To be completed by developer]

**Resolution:**  
[Description of fix applied]

**Fixed in Build/Version:** [e.g., v1.0.5]

**Resolved Date:** [MM/DD/YYYY]  
**Resolved By:** [Developer name]

---

## Retest

**Retest Required:** ☐ Yes  ☐ No

**Retested By:** [Tester name]  
**Retest Date:** [MM/DD/YYYY]  
**Retest Result:** ☐ Pass (issue resolved)  ☐ Fail (issue persists)  ☐ Regression (new issue found)

**Retest Notes:**  
[Details of retest execution and results]

---

## Closure

**Closed By:** [Name]  
**Closed Date:** [MM/DD/YYYY]  
**Final Disposition:** ☐ Fixed  ☐ Deferred to post-pilot  ☐ Won't Fix  ☐ Duplicate  ☐ Not a Defect

**Closure Notes:**  
[Reason for closure or deferral]

---

## Severity Guidelines

**Critical:**
- System crash or data loss
- Security vulnerability allowing unauthorized access
- Complete loss of major functionality (cannot log in, cannot create sponsors)
- Data corruption

**High:**
- Major feature broken (e.g., cannot activate LoG, approval workflow fails)
- Significant impact to workflow with no acceptable workaround
- Incorrect data displayed or saved
- Performance severely degraded (>30 second response time)

**Medium:**
- Feature partially working or has issues (e.g., filter doesn't work, validation too strict)
- Moderate usability issue
- Error message unclear or misleading
- Workaround exists but not ideal

**Low:**
- Cosmetic issue (typo, spacing, alignment)
- Minor inconvenience with easy workaround
- Enhancement request (not a defect)
- Help text missing or unclear

---

**Contact for Defect Discussion:**
- **Test Lead:** [Email]
- **Development Lead:** [Email]
- **Product Owner:** [Email]

---

**End of Defect Report**
