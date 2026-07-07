-- =============================================
-- Script: Add TargetAccountType column to auth.Authorization table
-- Date: 2026-07-07
-- Description: Add TargetAccountType field initialized to Client and constrained to Client, Prospect, or All.
-- =============================================

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[auth].[Authorization]')
    AND name = 'TargetAccountType'
)
BEGIN
    ALTER TABLE [auth].[Authorization]
    ADD [TargetAccountType] VARCHAR(20) NULL;

    PRINT 'Column TargetAccountType added to auth.Authorization table';
END
ELSE
BEGIN
    PRINT 'Column TargetAccountType already exists in auth.Authorization table';
END
GO

UPDATE [auth].[Authorization]
SET [TargetAccountType] = 'Client'
WHERE [TargetAccountType] IS NULL;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.default_constraints
    WHERE parent_object_id = OBJECT_ID(N'[auth].[Authorization]')
    AND name = 'DF_Authorization_TargetAccountType'
)
BEGIN
    ALTER TABLE [auth].[Authorization]
    ADD CONSTRAINT [DF_Authorization_TargetAccountType]
    DEFAULT ('Client') FOR [TargetAccountType];

    PRINT 'Default constraint DF_Authorization_TargetAccountType added';
END
ELSE
BEGIN
    PRINT 'Default constraint DF_Authorization_TargetAccountType already exists';
END
GO

ALTER TABLE [auth].[Authorization]
ALTER COLUMN [TargetAccountType] VARCHAR(20) NOT NULL;
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.check_constraints
    WHERE parent_object_id = OBJECT_ID(N'[auth].[Authorization]')
    AND name = 'CHK_Authorization_TargetAccountType'
)
BEGIN
    ALTER TABLE [auth].[Authorization]
    ADD CONSTRAINT [CHK_Authorization_TargetAccountType]
    CHECK ([TargetAccountType] IN ('Client', 'Prospect', 'All'));

    PRINT 'Check constraint CHK_Authorization_TargetAccountType added';
END
ELSE
BEGIN
    PRINT 'Check constraint CHK_Authorization_TargetAccountType already exists';
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'[auth].[Authorization]')
    AND name = 'IX_Authorization_Type_Configurable_TargetAccountType'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Authorization_Type_Configurable_TargetAccountType]
    ON [auth].[Authorization]([Type] ASC, [Configurable] ASC, [TargetAccountType] ASC);

    PRINT 'Index IX_Authorization_Type_Configurable_TargetAccountType added';
END
ELSE
BEGIN
    PRINT 'Index IX_Authorization_Type_Configurable_TargetAccountType already exists';
END
GO
