-- Fix Student Grade Levels to use STUD-XX format
-- This script converts existing grade level formats to the standard STUD-XX format
-- Run this script to update existing production data

PRINT 'Starting grade level conversion...';

-- Update "Grade 1" through "Grade 12" formats
UPDATE Students SET GradeLevel = 'STUD-01' WHERE GradeLevel IN ('Grade 1', '1', 'grade 1');
UPDATE Students SET GradeLevel = 'STUD-02' WHERE GradeLevel IN ('Grade 2', '2', 'grade 2');
UPDATE Students SET GradeLevel = 'STUD-03' WHERE GradeLevel IN ('Grade 3', '3', 'grade 3');
UPDATE Students SET GradeLevel = 'STUD-04' WHERE GradeLevel IN ('Grade 4', '4', 'grade 4');
UPDATE Students SET GradeLevel = 'STUD-05' WHERE GradeLevel IN ('Grade 5', '5', 'grade 5');
UPDATE Students SET GradeLevel = 'STUD-06' WHERE GradeLevel IN ('Grade 6', '6', 'grade 6');
UPDATE Students SET GradeLevel = 'STUD-07' WHERE GradeLevel IN ('Grade 7', '7', 'grade 7');
UPDATE Students SET GradeLevel = 'STUD-08' WHERE GradeLevel IN ('Grade 8', '8', 'grade 8');
UPDATE Students SET GradeLevel = 'STUD-09' WHERE GradeLevel IN ('Grade 9', '9', 'grade 9');
UPDATE Students SET GradeLevel = 'STUD-10' WHERE GradeLevel IN ('Grade 10', '10', 'grade 10');
UPDATE Students SET GradeLevel = 'STUD-11' WHERE GradeLevel IN ('Grade 11', '11', 'grade 11');
UPDATE Students SET GradeLevel = 'STUD-12' WHERE GradeLevel IN ('Grade 12', '12', 'grade 12');

-- Update Kindergarten/Preschool
UPDATE Students SET GradeLevel = 'STUD-KINDER' WHERE GradeLevel IN ('Kindergarten', 'Kinder', 'K', 'kindergarten', 'kinder');
UPDATE Students SET GradeLevel = 'STUD-P3' WHERE GradeLevel IN ('P3', 'Pre-K 3', 'PreK 3', 'p3');
UPDATE Students SET GradeLevel = 'STUD-P4' WHERE GradeLevel IN ('P4', 'Pre-K 4', 'PreK 4', 'p4');

PRINT 'Grade level conversion complete.';

-- Show summary of current grade levels
PRINT '';
PRINT 'Current grade level distribution:';
SELECT GradeLevel, COUNT(*) as StudentCount
FROM Students
GROUP BY GradeLevel
ORDER BY GradeLevel;
