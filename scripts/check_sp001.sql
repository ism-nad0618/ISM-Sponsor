/*
 * Check if Sponsor SP001 exists and find its associated user account
 */

-- Check if sponsor SP001 exists
SELECT 
    SponsorId, 
    SponsorName, 
    LegalName, 
    IsActive, 
    ApprovalStatus,
    CreatedOn
FROM Sponsors 
WHERE SponsorId = 'SP001';

-- Find user account linked to sponsor SP001
SELECT 
    Id,
    UserName,
    DisplayName,
    Email,
    SponsorId,
    IsActive
FROM AspNetUsers 
WHERE SponsorId = 'SP001';

-- List all sponsor users for reference
SELECT 
    UserName,
    DisplayName,
    SponsorId,
    IsActive
FROM AspNetUsers 
WHERE SponsorId IS NOT NULL
ORDER BY UserName;
