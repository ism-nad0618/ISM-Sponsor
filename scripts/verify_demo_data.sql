-- =====================================================
-- Quick Verification Script for Azure SQL
-- =====================================================
-- Run this after executing seed_medium_demo_data.sql
-- to verify all data was created successfully
-- =====================================================

SET NOCOUNT ON;

PRINT '';
PRINT '========================================';
PRINT 'Demo Data Verification';
PRINT '========================================';
PRINT '';

-- Check School Years
PRINT '1. School Years:';
SELECT SchoolYearId, Name, IsActive
FROM SchoolYears
WHERE SchoolYearId IN ('24-25', '25-26')
ORDER BY SchoolYearId;
PRINT '';

-- Check Categories
PRINT '2. Item Categories:';
SELECT CategoryId, CategoryName, IsActive
FROM ItemCategories
WHERE CategoryId IN ('TUITION', 'SUPPLIES', 'UNIFORM', 'ACTIVITIES', 'OTHER')
ORDER BY CategoryId;
PRINT '';

-- Check Items
PRINT '3. Items:';
SELECT ItemId, ItemName, GradeLevel, CategoryId
FROM Items
WHERE ItemId IN ('TUITION-ELEM', 'TUITION-HS', 'BOOKS', 'UNIFORM', 'PE-UNIFORM', 'SUPPLIES', 'MEALS', 'TRANSPORT')
ORDER BY ItemId;
PRINT '';

-- Check Demo Sponsors
PRINT '4. Demo Sponsors:';
SELECT 
    SponsorId,
    SponsorName,
    LegalName,
    IsActive,
    ApprovalStatus
FROM Sponsors
WHERE SponsorId LIKE 'DEMO-SP%'
ORDER BY SponsorId;
PRINT '';

-- Check Demo Students
PRINT '5. Demo Students (sample - first 10):';
SELECT TOP 10
    StudentId,
    FirstName,
    LastName,
    GradeLevel,
    SponsorId,
    StudentStatus
FROM Students
WHERE StudentId LIKE 'DEMO-ST%'
ORDER BY StudentId;
PRINT '';

-- Check Demo LoGs
PRINT '6. Demo Letters of Guarantee (sample - first 10):';
SELECT TOP 10
    LogId,
    StudentId,
    SponsorId,
    LogStatus,
    IsActive,
    EffectiveFrom,
    EffectiveTo
FROM LogCoverages
WHERE StudentId LIKE 'DEMO-ST%'
ORDER BY StudentId;
PRINT '';

-- Check Coverage Rules
PRINT '7. Coverage Rules (sample - first 10):';
SELECT TOP 10
    RuleId,
    LogId,
    CoverageTarget,
    ItemId,
    CoverageType,
    CoveragePercentage,
    CoverageFixedAmount,
    CapAmount
FROM LoGCoverageRules
WHERE LogId IN (SELECT LogId FROM LogCoverages WHERE StudentId LIKE 'DEMO-ST%')
ORDER BY RuleId;
PRINT '';

-- Check Demo Users
PRINT '8. Demo Users:';
SELECT 
    UserName,
    Email,
    EmailConfirmed,
    LockoutEnabled
FROM AspNetUsers
WHERE Email LIKE 'demo.%@ismanila.org'
ORDER BY Email;
PRINT '';

-- Summary Counts
PRINT '========================================';
PRINT 'Summary Counts';
PRINT '========================================';

DECLARE @SponsorCount INT, @StudentCount INT, @LogCount INT, @RuleCount INT, @UserCount INT;

SELECT @SponsorCount = COUNT(*) FROM Sponsors WHERE SponsorId LIKE 'DEMO-SP%';
SELECT @StudentCount = COUNT(*) FROM Students WHERE StudentId LIKE 'DEMO-ST%';
SELECT @LogCount = COUNT(*) FROM LogCoverages WHERE StudentId LIKE 'DEMO-ST%';
SELECT @RuleCount = COUNT(*) FROM LoGCoverageRules 
WHERE LogId IN (SELECT LogId FROM LogCoverages WHERE StudentId LIKE 'DEMO-ST%');
SELECT @UserCount = COUNT(*) FROM AspNetUsers WHERE Email LIKE 'demo.%@ismanila.org';

PRINT 'Sponsors Created: ' + CAST(@SponsorCount AS VARCHAR) + ' (Expected: 10)';
PRINT 'Students Created: ' + CAST(@StudentCount AS VARCHAR) + ' (Expected: 100)';
PRINT 'LoGs Created: ' + CAST(@LogCount AS VARCHAR) + ' (Expected: 100)';
PRINT 'Coverage Rules Created: ' + CAST(@RuleCount AS VARCHAR) + ' (Expected: 200+)';
PRINT 'Demo Users Created: ' + CAST(@UserCount AS VARCHAR) + ' (Expected: 20)';
PRINT '';

-- Validation
IF @SponsorCount = 10 AND @StudentCount = 100 AND @LogCount = 100 AND @RuleCount >= 200 AND @UserCount = 20
BEGIN
    PRINT '✅ All demo data created successfully!';
END
ELSE
BEGIN
    PRINT '⚠️ Some data may be missing. Review counts above.';
END

SET NOCOUNT OFF;
