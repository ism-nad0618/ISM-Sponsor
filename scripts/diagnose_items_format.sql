-- ====================================================================
-- ISM Sponsor - Items Table Format Diagnostic Script
-- Purpose: Diagnose ItemId format mismatches in coverage rules save
-- ====================================================================
-- Run these queries on your production database to identify the exact
-- format of ItemIds, especially for STUD-10 grade level items.
-- ====================================================================

-- Query 1: Show all Items for STUD-10 grade level with exact format
-- This will help verify the ItemId format for Grade 10 items
SELECT TOP 20 
    ItemId, 
    ItemName, 
    GradeLevel, 
    CategoryId,
    LEN(ItemId) as ItemIdLength,
    IsActive
FROM Items 
WHERE GradeLevel = 'STUD-10' 
  AND IsActive = 1
ORDER BY ItemId;

-- Query 2: Search for all TUITION-related items
-- This will find items containing "TUITION" in their ItemId
SELECT TOP 30
    ItemId, 
    ItemName, 
    GradeLevel, 
    CategoryId,
    LEN(ItemId) as ItemIdLength
FROM Items 
WHERE ItemId LIKE '%TUITION%' 
  AND IsActive = 1
ORDER BY GradeLevel, ItemId;

-- Query 3: Check for the specific ItemId mentioned by user
-- This should return 1 row if the exact ItemId exists
SELECT 
    ItemId, 
    ItemName, 
    GradeLevel, 
    CategoryId,
    LEN(ItemId) as ItemIdLength,
    IsActive
FROM Items 
WHERE ItemId = 'MAJOR-TUITION-PHP-G10 TUITION FULL';

-- Query 4: Check for similar ItemIds with case-insensitive search
-- This will find items that match case-insensitively
SELECT 
    ItemId, 
    ItemName, 
    GradeLevel, 
    CategoryId,
    LEN(ItemId) as ItemIdLength
FROM Items 
WHERE UPPER(ItemId) = UPPER('MAJOR-TUITION-PHP-G10 TUITION FULL');

-- Query 5: Check for ItemIds with similar patterns (PHP tuition for G10)
-- This uses pattern matching to find similar items
SELECT 
    ItemId, 
    ItemName, 
    GradeLevel, 
    CategoryId,
    LEN(ItemId) as ItemIdLength
FROM Items 
WHERE ItemId LIKE '%PHP%G10%' 
   OR ItemId LIKE '%TUITION%PHP%'
ORDER BY ItemId;

-- Query 6: Count items by grade level
-- This shows the distribution of items across grade levels
SELECT 
    GradeLevel, 
    COUNT(*) as ItemCount
FROM Items 
WHERE IsActive = 1
GROUP BY GradeLevel
ORDER BY GradeLevel;

-- Query 7: Show items with special characters or extra spaces
-- This will help identify formatting issues
SELECT 
    ItemId,
    ItemName,
    GradeLevel,
    LEN(ItemId) as OriginalLength,
    LEN(LTRIM(RTRIM(ItemId))) as TrimmedLength,
    CASE 
        WHEN LEN(ItemId) != LEN(LTRIM(RTRIM(ItemId))) 
        THEN 'HAS LEADING/TRAILING SPACES'
        ELSE 'Clean'
    END as SpaceCheck
FROM Items
WHERE GradeLevel = 'STUD-10'
  AND IsActive = 1
ORDER BY ItemId;

-- Query 8: Show all ItemIds that contain both "MAJOR" and "TUITION"
-- This narrows down to the specific pattern
SELECT 
    ItemId,
    ItemName,
    GradeLevel,
    CategoryId,
    LEN(ItemId) as ItemIdLength
FROM Items
WHERE ItemId LIKE '%MAJOR%'
  AND ItemId LIKE '%TUITION%'
  AND IsActive = 1
ORDER BY GradeLevel, ItemId;

-- Query 9: Character-by-character breakdown of specific ItemId
-- This will show exact byte representation if item exists
SELECT 
    ItemId,
    SUBSTRING(ItemId, 1, 10) as First10Chars,
    SUBSTRING(ItemId, LEN(ItemId)-9, 10) as Last10Chars,
    CHARINDEX(' ', ItemId) as FirstSpacePosition,
    LEN(ItemId) as TotalLength
FROM Items
WHERE ItemId LIKE '%MAJOR-TUITION-PHP-G10%'
   OR ItemId LIKE '%TUITION FULL%';

-- ====================================================================
-- INSTRUCTIONS:
-- 1. Run each query separately in your database management tool
-- 2. Copy the results, especially from Query 1, 2, and 3
-- 3. Pay special attention to:
--    - Exact ItemId format (spaces, hyphens, uppercase/lowercase)
--    - ItemIdLength values
--    - Any leading or trailing spaces (Query 7)
-- 4. Share the results so we can identify the exact mismatch
-- ====================================================================
