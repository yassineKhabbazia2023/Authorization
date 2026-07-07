-- =============================================
-- Script: Add AccountType column to account.Account table
-- Date: 2026-07-06
-- Description: Add AccountType field (NULL). Will be populated by DataFactory.
-- =============================================

-- Add AccountType column
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[account].[Account]')
    AND name = 'AccountType'
)
BEGIN
    ALTER TABLE [account].[Account]
    ADD [AccountType] VARCHAR(50) NULL;

    PRINT 'Column AccountType added to account.Account table';
END
ELSE
BEGIN
    PRINT 'Column AccountType already exists in account.Account table';
END
GO
