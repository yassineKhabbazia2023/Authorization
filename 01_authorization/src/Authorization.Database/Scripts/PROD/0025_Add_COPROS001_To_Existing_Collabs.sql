-- =============================================
-- Script: Assign COPROS001 to all existing collaborators
-- Date: 2026-07-10
-- Description: Adds global COPROS001 permission (AccountId = -1) to all existing collaborators
-- =============================================

BEGIN TRANSACTION;

-- Get COPROS001 permission ID
DECLARE @AuthorizationId INT;
SELECT @AuthorizationId = AuthorizationId
FROM [auth].[Authorization]
WHERE Code = 'COPROS001';

-- Check permission exists
IF @AuthorizationId IS NULL
BEGIN
    PRINT 'ERROR: Permission COPROS001 not found. Run 0024_Add_COPROS001_Permission.sql first.';
    ROLLBACK TRANSACTION;
    RETURN;
END

-- Add permission to all collaborators (AccountId = -1 for global permissions)
INSERT INTO [auth].[ContactAuthorization] (ContactId, AccountId, AuthorizationId, CreationDate)
SELECT
    c.ContactId,
    -1 AS AccountId,
    @AuthorizationId,
    GETDATE()
FROM [actor].[Contact] c
WHERE c.Type = 'collaborator'
    AND NOT EXISTS (
        SELECT 1
        FROM [auth].[ContactAuthorization] ca
        WHERE ca.ContactId = c.ContactId
            AND ca.AccountId = -1
            AND ca.AuthorizationId = @AuthorizationId
    );

-- Show result
DECLARE @AddedCount INT = @@ROWCOUNT;
PRINT 'COPROS001 permission added to ' + CAST(@AddedCount AS VARCHAR(10)) + ' collaborators';

-- Verify result
SELECT
    COUNT(DISTINCT ca.ContactId) AS CollaboratorsWithPermission,
    COUNT(DISTINCT c.ContactId) AS TotalCollaborators
FROM [actor].[Contact] c
LEFT JOIN [auth].[ContactAuthorization] ca
    ON c.ContactId = ca.ContactId
    AND ca.AccountId = -1
    AND ca.AuthorizationId = @AuthorizationId
WHERE c.Type = 'collaborator';

-- Uncomment to commit transaction
-- COMMIT TRANSACTION;

-- Uncomment to rollback (for testing)
ROLLBACK TRANSACTION;
