-- =============================================
-- Script: Add COPROS001 permission for collaborators
-- Date: 2026-07-10
-- Description: Add global permission for collaborators to view prospect folders
-- =============================================

-- COPROS001: Voir les dossiers prospects
IF (SELECT COUNT(*) FROM [auth].[Authorization] WHERE Code = 'COPROS001') = 0
BEGIN
    INSERT INTO [auth].[Authorization] (
        [Name], [Description], [Code], [Label], [Category], [Configurable], [View], [ProductCode], [Type], [TargetAccountType])
    VALUES
        ('View Prospect Folders', '', 'COPROS001', 'Voir les dossiers prospects', 'COLPROS', 0, 'Global', NULL, 'collaborator', 'Prospect')

    PRINT 'Permission COPROS001 added';
END
ELSE
BEGIN
    PRINT 'Permission COPROS001 already exists';
END
GO

-- Add COPROS001 to Account -1 (global account for collaborator permissions)
DECLARE @AuthorizationId INT;
SELECT @AuthorizationId = AuthorizationId
FROM [auth].[Authorization]
WHERE Code = 'COPROS001';

IF @AuthorizationId IS NOT NULL AND NOT EXISTS (
    SELECT 1
    FROM [auth].[AccountAuthorization]
    WHERE AccountId = -1 AND AuthorizationId = @AuthorizationId
)
BEGIN
    INSERT INTO [auth].[AccountAuthorization] (AccountId, AuthorizationId, Enabled)
    VALUES (-1, @AuthorizationId, 1);
    PRINT 'COPROS001 added to Account -1';
END
ELSE IF @AuthorizationId IS NOT NULL
BEGIN
    PRINT 'COPROS001 already exists for Account -1';
END
GO
