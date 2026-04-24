-- =====================================================
-- ISM Sponsor: Comprehensive Medium Demo Data Setup
-- =====================================================
-- Creates realistic demo data for UAT testing:
--   - 10 Sponsors (DEMO-SP001 to DEMO-SP010)
--   - 100 Students (DEMO-ST001 to DEMO-ST100, 10 per sponsor)
--   - 100 Letters of Guarantee with coverage rules
--   - 20 Demo Users (5 Admin, 5 Admissions, 5 Cashier, 5 Sponsor)
--   - Essential Items (Tuition, Books, Uniforms, etc.)
--
-- Usage: Execute against Azure SQL Database
-- Time: ~10-15 seconds
-- =====================================================

SET NOCOUNT ON;
PRINT 'Starting medium demo data setup...';
PRINT '';

-- =====================================================
-- STEP 1: Verify/Create School Years
-- =====================================================
PRINT '=== Setting up School Years ===';

IF NOT EXISTS (SELECT 1 FROM SchoolYears WHERE SchoolYearId = '24-25')
BEGIN
    INSERT INTO SchoolYears (SchoolYearId, Name, ValidFrom, ValidTo, IsActive)
    VALUES ('24-25', '2024-2025', '2024-08-15', '2025-05-30', 0);
    PRINT '  ✓ Created 2024-2025 school year';
END

IF NOT EXISTS (SELECT 1 FROM SchoolYears WHERE SchoolYearId = '25-26')
BEGIN
    INSERT INTO SchoolYears (SchoolYearId, Name, ValidFrom, ValidTo, IsActive)
    VALUES ('25-26', '2025-2026', '2025-08-15', '2026-05-30', 1);
    PRINT '  ✓ Created 2025-2026 school year (Active)';
END

PRINT '  ✓ School years ready';
PRINT '';

-- =====================================================
-- STEP 2: Create Categories and Items
-- =====================================================
PRINT '=== Setting up Items and Categories ===';

-- Categories
IF NOT EXISTS (SELECT 1 FROM ItemCategories WHERE CategoryId = 'TUITION')
BEGIN
    INSERT INTO ItemCategories (CategoryId, CategoryName, Description, IsActive)
    VALUES ('TUITION', 'Tuition Fees', 'Academic tuition and registration fees', 1);
    PRINT '  ✓ Created TUITION category';
END

IF NOT EXISTS (SELECT 1 FROM ItemCategories WHERE CategoryId = 'SUPPLIES')
BEGIN
    INSERT INTO ItemCategories (CategoryId, CategoryName, Description, IsActive)
    VALUES ('SUPPLIES', 'School Supplies', 'Books, materials, and supplies', 1);
    PRINT '  ✓ Created SUPPLIES category';
END

IF NOT EXISTS (SELECT 1 FROM ItemCategories WHERE CategoryId = 'UNIFORM')
BEGIN
    INSERT INTO ItemCategories (CategoryId, CategoryName, Description, IsActive)
    VALUES ('UNIFORM', 'Uniforms', 'School uniforms and PE attire', 1);
    PRINT '  ✓ Created UNIFORM category';
END

IF NOT EXISTS (SELECT 1 FROM ItemCategories WHERE CategoryId = 'ACTIVITIES')
BEGIN
    INSERT INTO ItemCategories (CategoryId, CategoryName, Description, IsActive)
    VALUES ('ACTIVITIES', 'Activities', 'Field trips, sports, clubs', 1);
    PRINT '  ✓ Created ACTIVITIES category';
END

IF NOT EXISTS (SELECT 1 FROM ItemCategories WHERE CategoryId = 'OTHER')
BEGIN
    INSERT INTO ItemCategories (CategoryId, CategoryName, Description, IsActive)
    VALUES ('OTHER', 'Other Fees', 'Miscellaneous school fees', 1);
    PRINT '  ✓ Created OTHER category';
END

-- Items
DECLARE @ItemsCreated INT = 0;

IF NOT EXISTS (SELECT 1 FROM Items WHERE ItemId = 'TUITION-ELEM')
BEGIN
    INSERT INTO Items (ItemId, ItemName, GradeLevel, CategoryId, Status, IsActive)
    VALUES ('TUITION-ELEM', 'Elementary Tuition', 'ES', 'TUITION', 'Active', 1);
    SET @ItemsCreated = @ItemsCreated + 1;
END

IF NOT EXISTS (SELECT 1 FROM Items WHERE ItemId = 'TUITION-HS')
BEGIN
    INSERT INTO Items (ItemId, ItemName, GradeLevel, CategoryId, Status, IsActive)
    VALUES ('TUITION-HS', 'High School Tuition', 'HS', 'TUITION', 'Active', 1);
    SET @ItemsCreated = @ItemsCreated + 1;
END

IF NOT EXISTS (SELECT 1 FROM Items WHERE ItemId = 'BOOKS')
BEGIN
    INSERT INTO Items (ItemId, ItemName, GradeLevel, CategoryId, Status, IsActive)
    VALUES ('BOOKS', 'Textbooks', 'ALL', 'SUPPLIES', 'Active', 1);
    SET @ItemsCreated = @ItemsCreated + 1;
END

IF NOT EXISTS (SELECT 1 FROM Items WHERE ItemId = 'UNIFORM')
BEGIN
    INSERT INTO Items (ItemId, ItemName, GradeLevel, CategoryId, Status, IsActive)
    VALUES ('UNIFORM', 'School Uniform', 'ALL', 'UNIFORM', 'Active', 1);
    SET @ItemsCreated = @ItemsCreated + 1;
END

IF NOT EXISTS (SELECT 1 FROM Items WHERE ItemId = 'PE-UNIFORM')
BEGIN
    INSERT INTO Items (ItemId, ItemName, GradeLevel, CategoryId, Status, IsActive)
    VALUES ('PE-UNIFORM', 'PE Uniform', 'ALL', 'UNIFORM', 'Active', 1);
    SET @ItemsCreated = @ItemsCreated + 1;
END

IF NOT EXISTS (SELECT 1 FROM Items WHERE ItemId = 'SUPPLIES')
BEGIN
    INSERT INTO Items (ItemId, ItemName, GradeLevel, CategoryId, Status, IsActive)
    VALUES ('SUPPLIES', 'School Supplies', 'ALL', 'SUPPLIES', 'Active', 1);
    SET @ItemsCreated = @ItemsCreated + 1;
END

IF NOT EXISTS (SELECT 1 FROM Items WHERE ItemId = 'MEALS')
BEGIN
    INSERT INTO Items (ItemId, ItemName, GradeLevel, CategoryId, Status, IsActive)
    VALUES ('MEALS', 'Meal Plan', 'ALL', 'OTHER', 'Active', 1);
    SET @ItemsCreated = @ItemsCreated + 1;
END

IF NOT EXISTS (SELECT 1 FROM Items WHERE ItemId = 'TRANSPORT')
BEGIN
    INSERT INTO Items (ItemId, ItemName, GradeLevel, CategoryId, Status, IsActive)
    VALUES ('TRANSPORT', 'Transportation', 'ALL', 'OTHER', 'Active', 1);
    SET @ItemsCreated = @ItemsCreated + 1;
END

PRINT '  ✓ Created ' + CAST(@ItemsCreated AS VARCHAR) + ' new items';
PRINT '';

-- =====================================================
-- STEP 3: Create 10 Demo Sponsors
-- =====================================================
PRINT '=== Creating 10 Demo Sponsors ===';

DECLARE @SponsorNames TABLE (Num INT, CompanyName NVARCHAR(100), ContactName NVARCHAR(100), Email NVARCHAR(100));
INSERT INTO @SponsorNames VALUES
(1, 'Tech Innovations Corp', 'Maria Santos', 'maria.santos@techinnovations.ph'),
(2, 'Global Finance Bank', 'Robert Chen', 'robert.chen@globalfinance.com'),
(3, 'PhilHealth Foundation', 'Ana Rodriguez', 'ana.rodriguez@philhealth.org'),
(4, 'Manila Trading Company', 'Jose Reyes', 'jose.reyes@manilatrading.ph'),
(5, 'Scholarship Trust Fund', 'Isabel Cruz', 'isabel.cruz@scholarshiptrust.org'),
(6, 'Community Outreach Org', 'Carlos Garcia', 'carlos.garcia@communityoutreach.ph'),
(7, 'Education Partners Inc', 'Linda Tan', 'linda.tan@educationpartners.com'),
(8, 'Youth Development Fund', 'Michael Fernandez', 'michael.f@youthdevelopment.org'),
(9, 'Corporate Social Resp', 'Patricia Lim', 'patricia.lim@csrfund.com'),
(10, 'Alumni Association', 'David Santiago', 'david.santiago@ismalumni.org');

DECLARE @i INT = 1;
DECLARE @CompanyName NVARCHAR(100), @ContactName NVARCHAR(100), @Email NVARCHAR(100);
DECLARE @SponsorId NVARCHAR(50);

WHILE @i <= 10
BEGIN
    SELECT @SponsorId = 'DEMO-SP' + RIGHT('000' + CAST(@i AS VARCHAR), 3);
    
    IF NOT EXISTS (SELECT 1 FROM Sponsors WHERE SponsorId = @SponsorId)
    BEGIN
        SELECT 
            @CompanyName = CompanyName,
            @ContactName = ContactName,
            @Email = Email
        FROM @SponsorNames WHERE Num = @i;
        
        INSERT INTO Sponsors (SponsorId, SponsorName, LegalName, Address, Tin, 
                             IsActive, CreatedOn, CreatedByUserId, PowerSchoolId, NetSuiteId, ApprovalStatus)
        VALUES (
            @SponsorId,
            @CompanyName,
            @CompanyName,
            CAST(@i AS VARCHAR) + ' Sponsor Avenue, BGC, Taguig City',
            CAST(100 + @i AS VARCHAR) + '-456-789-000',
            1,
            DATEADD(MONTH, -@i, GETDATE()),
            'system',
            'PS' + RIGHT('000' + CAST(@i AS VARCHAR), 3),
            'NS' + RIGHT('000' + CAST(@i AS VARCHAR), 3),
            'Approved'
        );
        
        PRINT '  ✓ Created ' + @SponsorId + ' - ' + @CompanyName;
    END
    
    SET @i = @i + 1;
END

PRINT '';

-- =====================================================
-- STEP 4: Create 100 Demo Students (10 per sponsor)
-- =====================================================
PRINT '=== Creating 100 Demo Students ===';

DECLARE @FirstNames TABLE (Name NVARCHAR(50));
INSERT INTO @FirstNames VALUES
('Juan'),('Maria'),('Jose'),('Ana'),('Pedro'),('Isabel'),('Carlos'),('Rosa'),('Miguel'),('Sofia'),
('Luis'),('Carmen'),('Antonio'),('Teresa'),('Francisco'),('Patricia'),('Manuel'),('Elena'),('Ramon'),('Lucia');

DECLARE @LastNames TABLE (Name NVARCHAR(50));
INSERT INTO @LastNames VALUES
('Santos'),('Reyes'),('Cruz'),('Garcia'),('Rodriguez'),('Fernandez'),('Lopez'),('Martinez'),('Gonzales'),('Perez');

DECLARE @StudentsCreated INT = 0;
DECLARE @StudentId NVARCHAR(50);
DECLARE @FirstName NVARCHAR(50), @LastName NVARCHAR(50);
DECLARE @GradeLevel INT;
DECLARE @CurrentSponsorId NVARCHAR(50);
DECLARE @j INT;

SET @i = 1;
WHILE @i <= 100
BEGIN
    SET @StudentId = 'DEMO-ST' + RIGHT('000' + CAST(@i AS VARCHAR), 3);
    
    IF NOT EXISTS (SELECT 1 FROM Students WHERE StudentId = @StudentId)
    BEGIN
        -- Assign sponsor (10 students per sponsor)
        SET @CurrentSponsorId = 'DEMO-SP' + RIGHT('000' + CAST(((@i - 1) / 10) + 1 AS VARCHAR), 3);
        
        -- Rotate through first and last names
        SET @j = ((@i - 1) % 20) + 1;
        SELECT @FirstName = Name FROM (
            SELECT Name, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM @FirstNames
        ) t WHERE rn = @j;
        
        SET @j = ((@i - 1) % 10) + 1;
        SELECT @LastName = Name FROM (
            SELECT Name, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM @LastNames
        ) t WHERE rn = @j;
        
        -- Grade levels 1-12
        SET @GradeLevel = ((@i - 1) % 12) + 1;
        
        INSERT INTO Students (SchoolYearId, StudentId, FirstName, LastName, GradeLevel, 
                             SponsorId, StudentStatus)
        VALUES (
            '25-26',
            @StudentId,
            @FirstName,
            @LastName,
            CASE WHEN @GradeLevel <= 6 THEN 'Grade ' + CAST(@GradeLevel AS VARCHAR)
                 ELSE 'Grade ' + CAST(@GradeLevel AS VARCHAR) END,
            @CurrentSponsorId,
            'Active'
        );
        
        SET @StudentsCreated = @StudentsCreated + 1;
    END
    
    SET @i = @i + 1;
END

PRINT '  ✓ Created ' + CAST(@StudentsCreated AS VARCHAR) + ' students (10 per sponsor)';
PRINT '';

-- =====================================================
-- STEP 5: Create Letters of Guarantee with Rules
-- =====================================================
PRINT '=== Creating Letters of Guarantee with Coverage Rules ===';

DECLARE @LogId INT;
DECLARE @LoGsCreated INT = 0;
DECLARE @RulesCreated INT = 0;

SET @i = 1;
WHILE @i <= 100
BEGIN
    SET @StudentId = 'DEMO-ST' + RIGHT('000' + CAST(@i AS VARCHAR), 3);
    SET @CurrentSponsorId = 'DEMO-SP' + RIGHT('000' + CAST(((@i - 1) / 10) + 1 AS VARCHAR), 3);
    
    -- Create LoG if not exists
    IF NOT EXISTS (SELECT 1 FROM LogCoverages WHERE StudentId = @StudentId AND SchoolYearId = '25-26')
    BEGIN
        INSERT INTO LogCoverages (SchoolYearId, StudentId, SponsorId, LogStatus, 
                                 IsActive, EffectiveFrom, EffectiveTo, Notes, CreatedOn)
        VALUES (
            '25-26',
            @StudentId,
            @CurrentSponsorId,
            CASE 
                WHEN @i % 10 = 0 THEN 'UnderReview'
                WHEN @i % 15 = 0 THEN 'Draft'
                ELSE 'Approved'
            END,
            CASE WHEN @i % 15 = 0 THEN 0 ELSE 1 END,
            '2025-08-15',
            '2026-05-30',
            'Demo LoG for UAT testing - Student ' + @StudentId,
            DATEADD(DAY, -30, GETDATE())
        );
        
        SET @LogId = SCOPE_IDENTITY();
        SET @LoGsCreated = @LoGsCreated + 1;
        
        -- Add coverage rules for this LoG
        -- Rule 1: Tuition (full or partial)
        INSERT INTO LoGCoverageRules (LogId, CoverageTarget, ItemId, CoverageType, 
                                     CoverageFixedAmount, CoveragePercentage, CapAmount, 
                                     ExceptionNote, IsActive, CreatedOn)
        VALUES (
            @LogId,
            'Item',
            CASE WHEN @i % 3 = 0 THEN 'TUITION-HS' ELSE 'TUITION-ELEM' END,
            CASE WHEN @i % 4 = 0 THEN 'FixedAmount' ELSE 'Full' END,
            CASE WHEN @i % 4 = 0 THEN 25000.00 ELSE NULL END,
            CASE WHEN @i % 4 = 0 THEN NULL ELSE 100.00 END,
            CASE WHEN @i % 3 = 0 THEN 55000.00 ELSE 45000.00 END,
            'Academic fees for SY 2025-2026',
            1,
            GETDATE()
        );
        SET @RulesCreated = @RulesCreated + 1;
        
        -- Rule 2: Books (if tuition is full coverage)
        IF @i % 4 <> 0
        BEGIN
            INSERT INTO LoGCoverageRules (LogId, CoverageTarget, ItemId, CoverageType, 
                                         CoveragePercentage, CapAmount, ExceptionNote, 
                                         IsActive, CreatedOn)
            VALUES (
                @LogId,
                'Item',
                'BOOKS',
                'Percentage',
                100.00,
                3500.00,
                'Required textbooks',
                1,
                GETDATE()
            );
            SET @RulesCreated = @RulesCreated + 1;
        END
        
        -- Rule 3: Uniform (for some students)
        IF @i % 3 = 0
        BEGIN
            INSERT INTO LoGCoverageRules (LogId, CoverageTarget, ItemId, CoverageType, 
                                         CoverageFixedAmount, CapAmount, ExceptionNote, 
                                         IsActive, CreatedOn)
            VALUES (
                @LogId,
                'Item',
                'UNIFORM',
                'FixedAmount',
                2500.00,
                2500.00,
                'Standard school uniform',
                1,
                GETDATE()
            );
            SET @RulesCreated = @RulesCreated + 1;
        END
    END
    
    SET @i = @i + 1;
END

PRINT '  ✓ Created ' + CAST(@LoGsCreated AS VARCHAR) + ' Letters of Guarantee';
PRINT '  ✓ Created ' + CAST(@RulesCreated AS VARCHAR) + ' coverage rules';
PRINT '';

-- =====================================================
-- STEP 6: Create Demo Users (5 per role = 20 users)
-- =====================================================
PRINT '=== Creating 20 Demo Users (5 per role) ===';

-- Note: Passwords need to be hashed by the application
-- This script creates the user records - passwords must be set via app

DECLARE @UserRoles TABLE (RoleId INT, RoleName NVARCHAR(50));
INSERT INTO @UserRoles VALUES
(1, 'Admin'),
(2, 'Admissions'),
(3, 'Cashier'),
(4, 'Sponsor');

DECLARE @UserFirstNames TABLE (Name NVARCHAR(50));
INSERT INTO @UserFirstNames VALUES
('Alice'),('Bob'),('Claire'),('David'),('Emma');

DECLARE @RoleId INT, @RoleName NVARCHAR(50);
DECLARE @UserNum INT;
DECLARE @Username NVARCHAR(100), @UserEmail NVARCHAR(100);
DECLARE @UsersCreated INT = 0;

DECLARE user_cursor CURSOR FOR SELECT RoleId, RoleName FROM @UserRoles;
OPEN user_cursor;
FETCH NEXT FROM user_cursor INTO @RoleId, @RoleName;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @UserNum = 1;
    WHILE @UserNum <= 5
    BEGIN
        SELECT @FirstName = Name FROM (
            SELECT Name, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM @UserFirstNames
        ) t WHERE rn = @UserNum;
        
        SET @Username = 'demo.' + LOWER(@RoleName) + CAST(@UserNum AS VARCHAR);
        SET @UserEmail = @Username + '@ismanila.org';
        
        IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = @UserEmail)
        BEGIN
            -- Create user record (password hash will be set by app)
            -- Using a placeholder GUID for Id
            DECLARE @UserId NVARCHAR(450) = LOWER(NEWID());
            
            INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail,
                                    EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled,
                                    LockoutEnabled, AccessFailedCount)
            VALUES (
                @UserId,
                @UserEmail,
                UPPER(@UserEmail),
                @UserEmail,
                UPPER(@UserEmail),
                1, -- Email confirmed
                0, -- Phone not confirmed
                0, -- 2FA disabled
                1, -- Lockout enabled
                0  -- No failed attempts
            );
            
            -- Assign role (note: requires AspNetRoles to exist)
            DECLARE @AspNetRoleId NVARCHAR(450);
            SELECT @AspNetRoleId = Id FROM AspNetRoles WHERE Name = @RoleName;
            
            IF @AspNetRoleId IS NOT NULL
            BEGIN
                INSERT INTO AspNetUserRoles (UserId, RoleId)
                VALUES (@UserId, @AspNetRoleId);
            END
            
            SET @UsersCreated = @UsersCreated + 1;
            PRINT '  ✓ Created user: ' + @UserEmail + ' (Role: ' + @RoleName + ')';
        END
        
        SET @UserNum = @UserNum + 1;
    END
    
    FETCH NEXT FROM user_cursor INTO @RoleId, @RoleName;
END

CLOSE user_cursor;
DEALLOCATE user_cursor;

PRINT '';
PRINT '  ⚠️  NOTE: User passwords must be set via application';
PRINT '  ⚠️  Default password suggestion: DemoPass123!';
PRINT '';

-- =====================================================
-- STEP 7: Summary Report
-- =====================================================
PRINT '===============================================';
PRINT 'Medium Demo Data Setup Complete!';
PRINT '===============================================';
PRINT '';

SELECT 'Sponsors' AS EntityType, COUNT(*) AS Count
FROM Sponsors WHERE SponsorId LIKE 'DEMO-%'
UNION ALL
SELECT 'Students', COUNT(*)
FROM Students WHERE StudentId LIKE 'DEMO-%'
UNION ALL
SELECT 'Letters of Guarantee', COUNT(*)
FROM LogCoverages WHERE SponsorId LIKE 'DEMO-%'
UNION ALL
SELECT 'Coverage Rules', COUNT(*)
FROM LoGCoverageRules 
WHERE LogId IN (SELECT LogId FROM LogCoverages WHERE SponsorId LIKE 'DEMO-%')
UNION ALL
SELECT 'Demo Users', COUNT(*)
FROM AspNetUsers WHERE Email LIKE 'demo.%@ismanila.org'
UNION ALL
SELECT 'Active Items', COUNT(*)
FROM Items WHERE IsActive = 1;

PRINT '';
PRINT '✅ Demo data ready for UAT testing!';
PRINT '';
PRINT 'Demo User Accounts (Password: DemoPass123!):';
PRINT '  Admins: demo.admin1 through demo.admin5@ismanila.org';
PRINT '  Admissions: demo.admissions1 through demo.admissions5@ismanila.org';
PRINT '  Cashiers: demo.cashier1 through demo.cashier5@ismanila.org';
PRINT '  Sponsors: demo.sponsor1 through demo.sponsor5@ismanila.org';
PRINT '';
PRINT 'Next Steps:';
PRINT '  1. Set passwords for demo users via Admin UI';
PRINT '  2. Test login with each role';
PRINT '  3. Verify data appears correctly in application';
PRINT '  4. Begin UAT testing with prepared test scripts';

SET NOCOUNT OFF;
