-- =====================================================
-- ISM Sponsor: Clean Demo Data from Database
-- =====================================================
-- This script removes all demo data to start fresh
-- 
-- WARNING: This will delete:
--   - All demo students (DEMO-ST*)
--   - All demo sponsors (DEMO-SP*)
--   - All demo LoGs and coverage rules
--   - All demo users (demo.*)
--
-- DOES NOT delete:
--   - School years
--   - Items and categories
--   - Regular production data
--
-- Usage: Execute against Azure SQL Database
-- Time: ~2-5 seconds
-- =====================================================

SET NOCOUNT ON;
PRINT 'Starting demo data cleanup...';
PRINT '';

-- =====================================================
-- STEP 1: Count current demo data
-- =====================================================
PRINT '=== Current Demo Data ===';

DECLARE @CurrentSponsors INT, @CurrentStudents INT, @CurrentLoGs INT, @CurrentRules INT;

SELECT @CurrentSponsors = COUNT(*) FROM Sponsors WHERE SponsorId LIKE 'DEMO-%';
SELECT @CurrentStudents = COUNT(*) FROM Students WHERE StudentId LIKE 'DEMO-%';
SELECT @CurrentLoGs = COUNT(*) FROM LogCoverages WHERE SponsorId LIKE 'DEMO-%';
SELECT @CurrentRules = COUNT(*) FROM LoGCoverageRules 
WHERE LogId IN (SELECT LogId FROM LogCoverages WHERE SponsorId LIKE 'DEMO-%');

PRINT 'Found:';
PRINT '  - Sponsors: ' + CAST(@CurrentSponsors AS VARCHAR);
PRINT '  - Students: ' + CAST(@CurrentStudents AS VARCHAR);
PRINT '  - LoGs: ' + CAST(@CurrentLoGs AS VARCHAR);
PRINT '  - Coverage Rules: ' + CAST(@CurrentRules AS VARCHAR);
PRINT '';

-- =====================================================
-- STEP 2: Delete LoG Coverage Rules
-- =====================================================
PRINT 'Deleting LoG coverage rules...';

DELETE FROM LoGCoverageRules
WHERE LogId IN (
    SELECT LogId 
    FROM LogCoverages 
    WHERE SponsorId LIKE 'DEMO-%'
);

PRINT '  ✓ Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' coverage rules';

-- =====================================================
-- STEP 3: Delete Coverage Evaluation Audits (if exists)
-- =====================================================
IF OBJECT_ID('CoverageEvaluationAudits', 'U') IS NOT NULL
BEGIN
    PRINT 'Deleting coverage evaluation audits...';
    
    DELETE FROM CoverageEvaluationAudits
    WHERE LogId IN (
        SELECT LogId 
        FROM LogCoverages 
        WHERE SponsorId LIKE 'DEMO-%'
    );
    
    PRINT '  ✓ Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' audit records';
END

-- =====================================================
-- STEP 4: Delete Letters of Guarantee
-- =====================================================
PRINT 'Deleting Letters of Guarantee...';

DELETE FROM LogCoverages
WHERE SponsorId LIKE 'DEMO-%';

PRINT '  ✓ Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' LoGs';

-- =====================================================
-- STEP 5: Delete Students
-- =====================================================
PRINT 'Deleting students...';

DELETE FROM Students
WHERE StudentId LIKE 'DEMO-%';

PRINT '  ✓ Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' students';

-- =====================================================
-- STEP 6: Delete Sponsor Change Requests (if exists)
-- =====================================================
IF OBJECT_ID('SponsorChangeRequests', 'U') IS NOT NULL
BEGIN
    PRINT 'Deleting sponsor change requests...';
    
    DELETE FROM SponsorChangeRequests
    WHERE SponsorId LIKE 'DEMO-%';
    
    PRINT '  ✓ Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' change requests';
END

-- =====================================================
-- STEP 7: Delete Sponsor Contacts and Addresses
-- =====================================================
PRINT 'Deleting sponsor contacts...';

DELETE FROM SponsorContacts
WHERE SponsorId LIKE 'DEMO-%';

PRINT '  ✓ Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' contacts';

IF OBJECT_ID('SponsorAddresses', 'U') IS NOT NULL
BEGIN
    PRINT 'Deleting sponsor addresses...';
    
    DELETE FROM SponsorAddresses
    WHERE SponsorId LIKE 'DEMO-%';
    
    PRINT '  ✓ Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' addresses';
END

-- =====================================================
-- STEP 8: Delete Sponsors
-- =====================================================
PRINT 'Deleting sponsors...';

DELETE FROM Sponsors
WHERE SponsorId LIKE 'DEMO-%';

PRINT '  ✓ Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' sponsors';

-- =====================================================
-- STEP 9: Delete Demo Users from AspNetUsers
-- =====================================================
PRINT 'Deleting demo users...';

-- First remove user roles
DELETE FROM AspNetUserRoles
WHERE UserId IN (
    SELECT Id FROM AspNetUsers 
    WHERE Email LIKE 'demo.%' OR UserName LIKE 'demo.%'
);

PRINT '  ✓ Removed user role assignments';

-- Then delete users
DELETE FROM AspNetUsers
WHERE Email LIKE 'demo.%' OR UserName LIKE 'demo.%';

PRINT '  ✓ Deleted ' + CAST(@@ROWCOUNT AS VARCHAR) + ' demo users';

-- =====================================================
-- STEP 10: Verification
-- =====================================================
PRINT '';
PRINT '===============================================';
PRINT 'Demo Data Cleanup Complete!';
PRINT '===============================================';

DECLARE @RemainingSponsors INT, @RemainingStudents INT, @RemainingLoGs INT;

SELECT @RemainingSponsors = COUNT(*) FROM Sponsors WHERE SponsorId LIKE 'DEMO-%';
SELECT @RemainingStudents = COUNT(*) FROM Students WHERE StudentId LIKE 'DEMO-%';
SELECT @RemainingLoGs = COUNT(*) FROM LogCoverages WHERE SponsorId LIKE 'DEMO-%';

PRINT 'Remaining Demo Data:';
PRINT '  - Sponsors: ' + CAST(@RemainingSponsors AS VARCHAR);
PRINT '  - Students: ' + CAST(@RemainingStudents AS VARCHAR);
PRINT '  - LoGs: ' + CAST(@RemainingLoGs AS VARCHAR);

IF @RemainingSponsors = 0 AND @RemainingStudents = 0 AND @RemainingLoGs = 0
BEGIN
    PRINT '';
    PRINT '✅ All demo data successfully removed!';
    PRINT 'Database is clean and ready for fresh demo data.';
END
ELSE
BEGIN
    PRINT '';
    PRINT '⚠️ Some demo data remains. Please review.';
END

PRINT '';
PRINT 'Note: School years, items, and categories were preserved.';
PRINT 'Note: Regular production data (non-DEMO prefixed) was not affected.';

SET NOCOUNT OFF;
