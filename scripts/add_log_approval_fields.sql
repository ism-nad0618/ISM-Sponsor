-- Migration: Add Letter of Guarantee Approval Workflow Fields
-- Run this on Azure SQL Database to fix the Dashboard page load error

-- Add approval workflow tracking columns (only if they don't exist)
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LogCoverages' AND COLUMN_NAME = 'SubmittedOn')
BEGIN
    ALTER TABLE [dbo].[LogCoverages] ADD [SubmittedOn] DATETIME2 NULL;
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LogCoverages' AND COLUMN_NAME = 'SubmittedByUserId')
BEGIN
    ALTER TABLE [dbo].[LogCoverages] ADD [SubmittedByUserId] NVARCHAR(450) NULL;
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LogCoverages' AND COLUMN_NAME = 'ReviewedOn')
BEGIN
    ALTER TABLE [dbo].[LogCoverages] ADD [ReviewedOn] DATETIME2 NULL;
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LogCoverages' AND COLUMN_NAME = 'ReviewedByUserId')
BEGIN
    ALTER TABLE [dbo].[LogCoverages] ADD [ReviewedByUserId] NVARCHAR(450) NULL;
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LogCoverages' AND COLUMN_NAME = 'ApprovedOn')
BEGIN
    ALTER TABLE [dbo].[LogCoverages] ADD [ApprovedOn] DATETIME2 NULL;
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LogCoverages' AND COLUMN_NAME = 'ApprovedByUserId')
BEGIN
    ALTER TABLE [dbo].[LogCoverages] ADD [ApprovedByUserId] NVARCHAR(450) NULL;
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LogCoverages' AND COLUMN_NAME = 'RejectedOn')
BEGIN
    ALTER TABLE [dbo].[LogCoverages] ADD [RejectedOn] DATETIME2 NULL;
END

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'LogCoverages' AND COLUMN_NAME = 'RejectedByUserId')
BEGIN
    ALTER TABLE [dbo].[LogCoverages] ADD [RejectedByUserId] NVARCHAR(450) NULL;
END

-- Note: ActivatedByUserId already exists, skipping

-- Add foreign key constraints (only if they don't exist)
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LogCoverages_AspNetUsers_SubmittedByUserId')
BEGIN
    ALTER TABLE [dbo].[LogCoverages]
    ADD CONSTRAINT [FK_LogCoverages_AspNetUsers_SubmittedByUserId]
    FOREIGN KEY ([SubmittedByUserId]) REFERENCES [dbo].[AspNetUsers]([Id])
    ON DELETE NO ACTION;
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LogCoverages_AspNetUsers_ReviewedByUserId')
BEGIN
    ALTER TABLE [dbo].[LogCoverages]
    ADD CONSTRAINT [FK_LogCoverages_AspNetUsers_ReviewedByUserId]
    FOREIGN KEY ([ReviewedByUserId]) REFERENCES [dbo].[AspNetUsers]([Id])
    ON DELETE NO ACTION;
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LogCoverages_AspNetUsers_ApprovedByUserId')
BEGIN
    ALTER TABLE [dbo].[LogCoverages]
    ADD CONSTRAINT [FK_LogCoverages_AspNetUsers_ApprovedByUserId]
    FOREIGN KEY ([ApprovedByUserId]) REFERENCES [dbo].[AspNetUsers]([Id])
    ON DELETE NO ACTION;
END

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_LogCoverages_AspNetUsers_RejectedByUserId')
BEGIN
    ALTER TABLE [dbo].[LogCoverages]
    ADD CONSTRAINT [FK_LogCoverages_AspNetUsers_RejectedByUserId]
    FOREIGN KEY ([RejectedByUserId]) REFERENCES [dbo].[AspNetUsers]([Id])
    ON DELETE NO ACTION;
END

-- Note: FK for ActivatedByUserId should already exist, skipping

-- Verify the columns were added
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'LogCoverages'
AND COLUMN_NAME IN (
    'SubmittedOn', 'SubmittedByUserId',
    'ReviewedOn', 'ReviewedByUserId',
    'ApprovedOn', 'ApprovedByUserId',
    'RejectedOn', 'RejectedByUserId',
    'ActivatedByUserId'
)
ORDER BY COLUMN_NAME;
