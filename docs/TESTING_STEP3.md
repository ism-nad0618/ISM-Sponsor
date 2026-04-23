# Step 3: Coverage Evaluation API - Testing Guide

## Manual API Testing

### 1. Test the Evaluate Endpoint

**Endpoint:** `POST /api/coverage/evaluate`

**Prerequisites:**
- Application running on https://localhost:5001
- Authenticated user with admin, admissions, or cashier role
- Active LoG with coverage rules in the database

**Sample Request (using curl):**

```bash
# First, login and get authentication cookie
curl -X POST https://localhost:5001/Account/Login \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=admin&password=admin" \
  -c cookies.txt

# Then, call the evaluate endpoint
curl -X POST https://localhost:5001/api/coverage/evaluate \
  -H "Content-Type: application/json" \
  -H "Cookie: $(cat cookies.txt | grep .AspNetCore.Cookies | awk '{print $6"="$7}')" \
  -d '{
    "schoolYearId": "25-26",
    "studentId": "S001",
    "itemId": "I1",
    "amount": 5000.00,
    "chargeDate": "2025-09-01T00:00:00"
  }'
```

**Expected Response (Full Coverage):**
```json
{
  "decision": "Covered",
  "billTo": "Sponsor",
  "sponsorAmount": 5000.00,
  "parentAmount": 0.00,
  "reasonCode": "FULL_COVERAGE_ITEM",
  "explanation": "Full coverage (100%) applied for item 'Item One'",
  "matchedRuleId": 1,
  "ruleVersion": "LOG1-RULE1-20260310123045",
  "auditRecordId": 1,
  "success": true,
  "errorMessage": null
}
```

### 2. Test the Reason Codes Endpoint

**Endpoint:** `GET /api/coverage/reasons`

```bash
curl -X GET https://localhost:5001/api/coverage/reasons \
  -H "Cookie: $(cat cookies.txt | grep .AspNetCore.Cookies | awk '{print $6"="$7}')"
```

## Test Scenarios

### Success Scenarios

#### 1. Full Coverage - Item Rule
- **Setup:** Create LoG with Full coverage rule for a specific item
- **Request:** Evaluate charge for that item
- **Expected:** Decision=Covered, BillTo=Sponsor, SponsorAmount=RequestedAmount

#### 2. Full Coverage - Category Rule
- **Setup:** Create LoG with Full coverage rule for a category
- **Request:** Evaluate charge for an item in that category
- **Expected:** Decision=Covered, BillTo=Sponsor, SponsorAmount=RequestedAmount

#### 3. Percentage Coverage (100%)
- **Setup:** Create LoG with 100% percentage coverage
- **Request:** Evaluate charge
- **Expected:** Decision=Covered, BillTo=Sponsor, SponsorAmount=RequestedAmount

#### 4. Percentage Coverage (< 100%)
- **Setup:** Create LoG with 75% percentage coverage
- **Request:** Evaluate charge for ₱10,000
- **Expected:** Decision=Split, BillTo=Split, SponsorAmount=₱7,500, ParentAmount=₱2,500

#### 5. Fixed Amount Coverage (Full)
- **Setup:** Create LoG with ₱5,000 fixed amount coverage
- **Request:** Evaluate charge for ₱4,000
- **Expected:** Decision=Covered, BillTo=Sponsor, SponsorAmount=₱4,000

#### 6. Fixed Amount Coverage (Partial)
- **Setup:** Create LoG with ₱5,000 fixed amount coverage
- **Request:** Evaluate charge for ₱8,000
- **Expected:** Decision=Split, BillTo=Split, SponsorAmount=₱5,000, ParentAmount=₱3,000

#### 7. Up To Cap Coverage (Within Cap)
- **Setup:** Create LoG with ₱10,000 cap
- **Request:** Evaluate charge for ₱8,000
- **Expected:** Decision=Covered, BillTo=Sponsor, SponsorAmount=₱8,000

#### 8. Up To Cap Coverage (Exceeds Cap)
- **Setup:** Create LoG with ₱10,000 cap
- **Request:** Evaluate charge for ₱15,000
- **Expected:** Decision=Split, BillTo=Split, SponsorAmount=₱10,000, ParentAmount=₱5,000

#### 9. Item Rule Overrides Category Rule
- **Setup:** Create LoG with 50% category coverage AND 100% item coverage for specific item in that category
- **Request:** Evaluate charge for that specific item
- **Expected:** Item rule applied (100% coverage), not category rule

#### 10. Effective Date Validation (Within Range)
- **Setup:** Create rule effective from 2025-09-01 to 2026-05-31
- **Request:** Evaluate charge dated 2025-10-15
- **Expected:** Rule applied successfully

### Failure Scenarios

#### 11. No Active LoG
- **Setup:** No LoG exists for student
- **Expected:** Decision=NotCovered, ReasonCode=NO_ACTIVE_LOG

#### 12. Inactive LoG
- **Setup:** LoG exists but IsActive=false
- **Expected:** Decision=NotCovered, ReasonCode=LOG_INACTIVE

#### 13. No Matching Rule
- **Setup:** LoG exists but no rules cover the requested item/category
- **Expected:** Decision=NotCovered, ReasonCode=NO_MATCHING_RULE

#### 14. Rule Not Yet Effective
- **Setup:** Rule with EffectiveFrom in the future
- **Request:** Evaluate charge with today's date
- **Expected:** Decision=NotCovered, ReasonCode=RULE_NOT_YET_EFFECTIVE

#### 15. Rule Expired
- **Setup:** Rule with EffectiveTo in the past
- **Request:** Evaluate charge with today's date
- **Expected:** Decision=NotCovered, ReasonCode=RULE_EXPIRED

#### 16. Inactive Rule
- **Setup:** Rule exists but IsActive=false
- **Expected:** Decision=NotCovered, ReasonCode=RULE_INACTIVE

### Validation Scenarios

#### 17. Missing Student ID
- **Request:** Omit studentId
- **Expected:** 400 Bad Request with model validation error

#### 18. Missing Amount
- **Request:** Omit amount
- **Expected:** 400 Bad Request with model validation error

#### 19. Zero Amount
- **Request:** amount = 0
- **Expected:** 400 Bad Request with validation error

#### 20. Negative Amount
- **Request:** amount = -100
- **Expected:** 400 Bad Request with validation error

#### 21. Missing Item and Category
- **Request:** Omit both itemId and categoryId
- **Expected:** Decision=NotCovered, ReasonCode=MISSING_ITEM_OR_CATEGORY

#### 22. Sponsor/LoG Mismatch
- **Setup:** LoG belongs to SponsorA
- **Request:** Provide SponsorB in request
- **Expected:** Decision=NotCovered, ReasonCode=SPONSOR_LOG_MISMATCH

### Authorization Scenarios

#### 23. Admin Role Access
- **User:** admin role
- **Expected:** 200 OK with evaluation result

#### 24. Admissions Role Access
- **User:** admissions role
- **Expected:** 200 OK with evaluation result

#### 25. Cashier Role Access
- **User:** cashier role
- **Expected:** 200 OK with evaluation result

#### 26. Sponsor Role Denied
- **User:** sponsor role
- **Expected:** 403 Forbidden

#### 27. Unauthenticated Access
- **User:** Not logged in
- **Expected:** 401 Unauthorized

### Audit Scenarios

#### 28. Success Audit Record
- **After:** Successful evaluation
- **Verify:** CoverageEvaluationAudits table contains record with Success=true, all request/response fields populated

#### 29. Failure Audit Record
- **After:** Failed evaluation (no LoG found)
- **Verify:** CoverageEvaluationAudits table contains record with Success=true (evaluation completed), Decision=NotCovered

#### 30. Error Audit Record
- **After:** System error during evaluation
- **Verify:** CoverageEvaluationAudits table contains record with Success=false, ErrorMessage populated

## Verification Queries

### Check Audit Records
```sql
SELECT TOP 10 
    AuditId,
    EvaluatedOn,
    EvaluatedByUserDisplay,
    StudentId,
    ItemId,
    RequestedAmount,
    Decision,
    BillTo,
    SponsorAmount,
    ParentAmount,
    ReasonCode,
    Explanation,
    MatchedRuleId,
    RuleVersion,
    Success
FROM CoverageEvaluationAudits
ORDER BY EvaluatedOn DESC;
```

### Check Active LoGs with Rules
```sql
SELECT 
    lc.LogId,
    lc.StudentId,
    lc.SponsorId,
    lc.IsActive AS LogActive,
    COUNT(lcr.RuleId) AS RuleCount
FROM LogCoverages lc
LEFT JOIN LoGCoverageRules lcr ON lc.LogId = lcr.LogId AND lcr.IsActive = 1
WHERE lc.SchoolYearId = '25-26'
GROUP BY lc.LogId, lc.StudentId, lc.SponsorId, lc.IsActive;
```

## Integration Testing Notes

For proper integration testing, create:
1. **Test fixtures** with known LoGs, rules, students, and items
2. **Test database** or use in-memory database
3. **HttpClient** for API endpoint testing
4. **Authentication helper** to obtain valid cookies/tokens
5. **Assertion helpers** to verify audit records created

## Performance Considerations

The "fast preview path" for common items is implemented in `FindMatchingRuleAsync`:
- Item rules are evaluated first (exact match)
- Category rules are evaluated second
- Item-to-category lookup is performed only if needed

For high-volume scenarios, consider:
- Caching active LoGs and rules
- Pre-computing common evaluation results
- Batch evaluation endpoint for multiple charges
