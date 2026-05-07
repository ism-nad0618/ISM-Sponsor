#!/bin/bash

# Manual Integration Tests for Coverage API Endpoints
# Run this script after starting the server with: dotnet run
# Expected server URL: https://localhost:5001

set -e

BASE_URL="https://localhost:5001"
RESULTS_FILE="/tmp/test_results.txt"

echo "========================================" | tee $RESULTS_FILE
echo "Coverage API Integration Tests" | tee -a $RESULTS_FILE
echo "========================================" | tee -a $RESULTS_FILE
echo "" | tee -a $RESULTS_FILE

# Test 1: POST /api/v1/coverage/preview with $1500 (EXCEEDS_CAP - Split billing)
echo "Test 1: POST /api/v1/coverage/preview - Split billing ($1500)" | tee -a $RESULTS_FILE
echo "---------------------------------------" | tee -a $RESULTS_FILE
RESPONSE=$(curl -k -s -X POST "${BASE_URL}/api/v1/coverage/preview" \
  -H "Content-Type: application/json" \
  -d '{
    "studentId": "STUD006",
    "schoolYearId": "25-26",
    "itemId": "MAJOR-SLSP-G06 SLSP FULL",
    "chargeDescription": "Specialized Learning Support Program Fee",
    "amount": 1500.00,
    "currency": "US Dollar",
    "chargeDate": "2026-05-06"
  }')

echo "$RESPONSE" | python3 -m json.tool > /tmp/test1.json 2>/dev/null || echo "$RESPONSE" > /tmp/test1.json
DECISION=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('decision', 'ERROR'))" 2>/dev/null || echo "ERROR")
BILL_TO=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('billTo', 'ERROR'))" 2>/dev/null || echo "ERROR")
SPONSOR_AMT=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('sponsorAmount', 0))" 2>/dev/null || echo "0")
PARENT_AMT=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('parentAmount', 0))" 2>/dev/null || echo "0")
ALLOC_COUNT=$(echo "$RESPONSE" | python3 -c "import sys, json; print(len(json.load(sys.stdin).get('allocations', [])))" 2>/dev/null || echo "0")

echo "  Decision: $DECISION (expected: 1=Split)" | tee -a $RESULTS_FILE
echo "  BillTo: $BILL_TO (expected: 3=SponsorAndParent)" | tee -a $RESULTS_FILE
echo "  Sponsor Amount: $SPONSOR_AMT (expected: 1000.0)" | tee -a $RESULTS_FILE
echo "  Parent Amount: $PARENT_AMT (expected: 500.0)" | tee -a $RESULTS_FILE
echo "  Allocations Count: $ALLOC_COUNT (expected: 2)" | tee -a $RESULTS_FILE

if [ "$DECISION" == "1" ] && [ "$BILL_TO" == "3" ] && [ "$ALLOC_COUNT" == "2" ]; then
    echo "  ✅ PASS" | tee -a $RESULTS_FILE
else
    echo "  ❌ FAIL" | tee -a $RESULTS_FILE
fi
echo "" | tee -a $RESULTS_FILE

# Test 2: POST /api/v1/coverage/evaluate with $1500 (EXCEEDS_CAP - Split billing)
echo "Test 2: POST /api/v1/coverage/evaluate - Split billing ($1500)" | tee -a $RESULTS_FILE
echo "---------------------------------------" | tee -a $RESULTS_FILE
RESPONSE=$(curl -k -s -X POST "${BASE_URL}/api/v1/coverage/evaluate" \
  -H "Content-Type: application/json" \
  -d '{
    "studentId": "STUD006",
    "schoolYearId": "25-26",
    "itemId": "MAJOR-SLSP-G06 SLSP FULL",
    "chargeDescription": "Specialized Learning Support Program Fee",
    "amount": 1500.00,
    "currency": "US Dollar",
    "chargeDate": "2026-05-06"
  }')

echo "$RESPONSE" | python3 -m json.tool > /tmp/test2.json 2>/dev/null || echo "$RESPONSE" > /tmp/test2.json
DECISION=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('decision', 'ERROR'))" 2>/dev/null || echo "ERROR")
BILL_TO=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('billTo', 'ERROR'))" 2>/dev/null || echo "ERROR")
SPONSOR_AMT=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('sponsorAmount', 0))" 2>/dev/null || echo "0")
PARENT_AMT=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('parentAmount', 0))" 2>/dev/null || echo "0")
ALLOC_COUNT=$(echo "$RESPONSE" | python3 -c "import sys, json; print(len(json.load(sys.stdin).get('allocations', [])))" 2>/dev/null || echo "0")

echo "  Decision: $DECISION (expected: 1=Split)" | tee -a $RESULTS_FILE
echo "  BillTo: $BILL_TO (expected: 3=SponsorAndParent)" | tee -a $RESULTS_FILE
echo "  Sponsor Amount: $SPONSOR_AMT (expected: 1000.0)" | tee -a $RESULTS_FILE
echo "  Parent Amount: $PARENT_AMT (expected: 500.0)" | tee -a $RESULTS_FILE
echo "  Allocations Count: $ALLOC_COUNT (expected: 2)" | tee -a $RESULTS_FILE

if [ "$DECISION" == "1" ] && [ "$BILL_TO" == "3" ] && [ "$ALLOC_COUNT" == "2" ]; then
    echo "  ✅ PASS" | tee -a $RESULTS_FILE
else
    echo "  ❌ FAIL" | tee -a $RESULTS_FILE
fi
echo "" | tee -a $RESULTS_FILE

# Test 3: POST /api/v1/coverage/preview with $800 (Fully covered)
echo "Test 3: POST /api/v1/coverage/preview - Fully covered ($800)" | tee -a $RESULTS_FILE
echo "---------------------------------------" | tee -a $RESULTS_FILE
RESPONSE=$(curl -k -s -X POST "${BASE_URL}/api/v1/coverage/preview" \
  -H "Content-Type: application/json" \
  -d '{
    "studentId": "STUD006",
    "schoolYearId": "25-26",
    "itemId": "MAJOR-SLSP-G06 SLSP FULL",
    "chargeDescription": "Specialized Learning Support Program Fee",
    "amount": 800.00,
    "currency": "US Dollar",
    "chargeDate": "2026-05-06"
  }')

echo "$RESPONSE" | python3 -m json.tool > /tmp/test3.json 2>/dev/null || echo "$RESPONSE" > /tmp/test3.json
DECISION=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('decision', 'ERROR'))" 2>/dev/null || echo "ERROR")
BILL_TO=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('billTo', 'ERROR'))" 2>/dev/null || echo "ERROR")
SPONSOR_AMT=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('sponsorAmount', 0))" 2>/dev/null || echo "0")
PARENT_AMT=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('parentAmount', 0))" 2>/dev/null || echo "0")
ALLOC_COUNT=$(echo "$RESPONSE" | python3 -c "import sys, json; print(len(json.load(sys.stdin).get('allocations', [])))" 2>/dev/null || echo "0")

echo "  Decision: $DECISION (expected: 0=Covered)" | tee -a $RESULTS_FILE
echo "  BillTo: $BILL_TO (expected: 0=Sponsor)" | tee -a $RESULTS_FILE
echo "  Sponsor Amount: $SPONSOR_AMT (expected: 800.0)" | tee -a $RESULTS_FILE
echo "  Parent Amount: $PARENT_AMT (expected: 0.0)" | tee -a $RESULTS_FILE
echo "  Allocations Count: $ALLOC_COUNT (expected: 1)" | tee -a $RESULTS_FILE

if [ "$DECISION" == "0" ] && [ "$BILL_TO" == "0" ] && [ "$ALLOC_COUNT" == "1" ]; then
    echo "  ✅ PASS" | tee -a $RESULTS_FILE
else
    echo "  ❌ FAIL" | tee -a $RESULTS_FILE
fi
echo "" | tee -a $RESULTS_FILE

# Test 4: POST /api/v1/coverage/evaluate with $800 (Fully covered)
echo "Test 4: POST /api/v1/coverage/evaluate - Fully covered ($800)" | tee -a $RESULTS_FILE
echo "---------------------------------------" | tee -a $RESULTS_FILE
RESPONSE=$(curl -k -s -X POST "${BASE_URL}/api/v1/coverage/evaluate" \
  -H "Content-Type: application/json" \
  -d '{
    "studentId": "STUD006",
    "schoolYearId": "25-26",
    "itemId": "MAJOR-SLSP-G06 SLSP FULL",
    "chargeDescription": "Specialized Learning Support Program Fee",
    "amount": 800.00,
    "currency": "US Dollar",
    "chargeDate": "2026-05-06"
  }')

echo "$RESPONSE" | python3 -m json.tool > /tmp/test4.json 2>/dev/null || echo "$RESPONSE" > /tmp/test4.json
DECISION=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('decision', 'ERROR'))" 2>/dev/null || echo "ERROR")
BILL_TO=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('billTo', 'ERROR'))" 2>/dev/null || echo "ERROR")
SPONSOR_AMT=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('sponsorAmount', 0))" 2>/dev/null || echo "0")
PARENT_AMT=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('parentAmount', 0))" 2>/dev/null || echo "0")
ALLOC_COUNT=$(echo "$RESPONSE" | python3 -c "import sys, json; print(len(json.load(sys.stdin).get('allocations', [])))" 2>/dev/null || echo "0")

echo "  Decision: $DECISION (expected: 0=Covered)" | tee -a $RESULTS_FILE
echo "  BillTo: $BILL_TO (expected: 0=Sponsor)" | tee -a $RESULTS_FILE
echo "  Sponsor Amount: $SPONSOR_AMT (expected: 800.0)" | tee -a $RESULTS_FILE
echo "  Parent Amount: $PARENT_AMT (expected: 0.0)" | tee -a $RESULTS_FILE
echo "  Allocations Count: $ALLOC_COUNT (expected: 1)" | tee -a $RESULTS_FILE

if [ "$DECISION" == "0" ] && [ "$BILL_TO" == "0" ] && [ "$ALLOC_COUNT" == "1" ]; then
    echo "  ✅ PASS" | tee -a $RESULTS_FILE
else
    echo "  ❌ FAIL" | tee -a $RESULTS_FILE
fi
echo "" | tee -a $RESULTS_FILE

# Test 5: GET /api/v1/sponsors/SP002 (Ayala Holdings)
echo "Test 5: GET /api/v1/sponsors/SP002 - Get Ayala Holdings" | tee -a $RESULTS_FILE
echo "---------------------------------------" | tee -a $RESULTS_FILE
RESPONSE=$(curl -k -s "${BASE_URL}/api/v1/sponsors/SP002")

echo "$RESPONSE" | python3 -m json.tool > /tmp/test5.json 2>/dev/null || echo "$RESPONSE" > /tmp/test5.json
SPONSOR_ID=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('sponsorId', 'ERROR'))" 2>/dev/null || echo "ERROR")
SPONSOR_NAME=$(echo "$RESPONSE" | python3 -c "import sys, json; print(json.load(sys.stdin).get('sponsorName', 'ERROR'))" 2>/dev/null || echo "ERROR")

echo "  Sponsor ID: $SPONSOR_ID (expected: SP002)" | tee -a $RESULTS_FILE
echo "  Sponsor Name: $SPONSOR_NAME (expected: Ayala Holdings)" | tee -a $RESULTS_FILE

if [ "$SPONSOR_ID" == "SP002" ] && [ "$SPONSOR_NAME" == "Ayala Holdings" ]; then
    echo "  ✅ PASS" | tee -a $RESULTS_FILE
else
    echo "  ❌ FAIL" | tee -a $RESULTS_FILE
fi
echo "" | tee -a $RESULTS_FILE

echo "========================================" | tee -a $RESULTS_FILE
echo "Test Results Summary" | tee -a $RESULTS_FILE
echo "========================================" | tee -a $RESULTS_FILE
echo "Full response JSON files saved in /tmp/test*.json" | tee -a $RESULTS_FILE
echo "Test results saved to: $RESULTS_FILE" | tee -a $RESULTS_FILE
echo "" | tee -a $RESULTS_FILE

# Count pass/fail
PASS_COUNT=$(grep "✅ PASS" $RESULTS_FILE | wc -l | tr -d ' ')
FAIL_COUNT=$(grep "❌ FAIL" $RESULTS_FILE | wc -l | tr -d ' ')

echo "Passed: $PASS_COUNT / 5" | tee -a $RESULTS_FILE
echo "Failed: $FAIL_COUNT / 5" | tee -a $RESULTS_FILE

if [ "$FAIL_COUNT" -eq "0" ]; then
    echo "" | tee -a $RESULTS_FILE
    echo "🎉 All tests passed!" | tee -a $RESULTS_FILE
    exit 0
else
    echo "" | tee -a $RESULTS_FILE
    echo "⚠️  Some tests failed. Check responses in /tmp/test*.json" | tee -a $RESULTS_FILE
    exit 1
fi
