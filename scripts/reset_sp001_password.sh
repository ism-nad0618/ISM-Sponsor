#!/bin/bash
# Script to reset SP001 password via API endpoint
# Run this script and it will keep trying until successful

echo "==================================================="
echo "SP001 Password Reset Script"
echo "==================================================="
echo ""
echo "This script will attempt to reset the password for SP001"
echo "It will retry every 30 seconds until successful"
echo "Press Ctrl+C to stop"
echo ""

USERNAME="SP001"
NEW_PASSWORD="Sponsor@123"
URL="https://ismsponsor.azurewebsites.net/api/health/reset-password"

ATTEMPT=1
MAX_ATTEMPTS=20  # Try for about 10 minutes

while [ $ATTEMPT -le $MAX_ATTEMPTS ]; do
    echo "Attempt $ATTEMPT of $MAX_ATTEMPTS..."
    
    RESPONSE=$(curl -s -X POST "$URL" \
        -H "Content-Type: application/json" \
        -d "{\"username\":\"$USERNAME\",\"newPassword\":\"$NEW_PASSWORD\"}" \
        -w "\nHTTP_CODE:%{http_code}")
    
    HTTP_CODE=$(echo "$RESPONSE" | grep "HTTP_CODE" | cut -d: -f2)
    BODY=$(echo "$RESPONSE" | grep -v "HTTP_CODE")
    
    if [ "$HTTP_CODE" = "200" ]; then
        echo ""
        echo "✅ SUCCESS! Password reset completed."
        echo ""
        echo "$BODY" | python3 -m json.tool 2>/dev/null || echo "$BODY"
        echo ""
        echo "You can now login with:"
        echo "  Username: $USERNAME"
        echo "  Password: $NEW_PASSWORD"
        echo "  URL: https://ismsponsor.azurewebsites.net"
        echo ""
        exit 0
    else
        echo "   Status: $HTTP_CODE - Deployment still in progress..."
        if [ $ATTEMPT -lt $MAX_ATTEMPTS ]; then
            echo "   Waiting 30 seconds before retry..."
            sleep 30
        fi
    fi
    
    ATTEMPT=$((ATTEMPT + 1))
done

echo ""
echo "❌ Maximum attempts reached. Deployment may still be in progress."
echo ""
echo "Alternative: Use Admin UI to reset password"
echo "  1. Login as admin (username: admin, password: Admin@123)"
echo "  2. Go to Settings → Users"
echo "  3. Edit SP001 user and set new password"
echo ""
