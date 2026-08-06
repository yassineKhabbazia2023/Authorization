-- =============================================
-- Script: Grant COINVO001 to collaborators with GED permissions
-- Date: 2026-07-10
-- Description: Adds global COINVO001 permission (AccountId = -1) to all collaborators with COGED0001 or COGED0002
-- =============================================

BEGIN TRANSACTION;

-- Get permission IDs
DECLARE @COINVO001Id INT;
DECLARE @COGED0001Id INT;
DECLARE @COGED0002Id INT;

SELECT @COINVO001Id = AuthorizationId FROM [auth].[Authorization] WHERE Code = 'COINVO001';
SELECT @COGED0001Id = AuthorizationId FROM [auth].[Authorization] WHERE Code = 'COGED0001';
SELECT @COGED0002Id = AuthorizationId FROM [auth].[Authorization] WHERE Code = 'COGED0002';

-- Check permissions exist
IF @COINVO001Id IS NULL
BEGIN
	PRINT 'ERROR: Permission COINVO001 not found. Run 0026_Update_CLINVO001_COINVO001_permissions.sql first.';
	ROLLBACK TRANSACTION;
	RETURN;
END

IF @COGED0001Id IS NULL AND @COGED0002Id IS NULL
BEGIN
	PRINT 'ERROR: Neither COGED0001 nor COGED0002 permissions found.';
	ROLLBACK TRANSACTION;
	RETURN;
END

-- Add COINVO001 (AccountId = -1 for global scope) to all collaborators with COGED0001 or COGED0002
INSERT INTO [auth].[ContactAuthorization] (ContactId, AccountId, AuthorizationId, CreationDate)
SELECT DISTINCT
	ca.ContactId,
	-1 AS AccountId,
	@COINVO001Id,
	GETUTCDATE()
FROM [auth].[ContactAuthorization] ca
WHERE ca.AccountId = -1
	AND ca.AuthorizationId IN (@COGED0001Id, @COGED0002Id)
	AND NOT EXISTS (
		SELECT 1
		FROM [auth].[ContactAuthorization] ca2
		WHERE ca2.ContactId = ca.ContactId
			AND ca2.AccountId = -1
			AND ca2.AuthorizationId = @COINVO001Id
	);

-- Show result
DECLARE @AddedCount INT = @@ROWCOUNT;
PRINT 'COINVO001 permission added to ' + CAST(@AddedCount AS VARCHAR(10)) + ' collaborators';

-- Verify result
SELECT
	COUNT(DISTINCT ca.ContactId) AS CollaboratorsWithCOINVO001,
	(SELECT COUNT(DISTINCT ca2.ContactId) 
	 FROM [auth].[ContactAuthorization] ca2 
	 WHERE ca2.AccountId = -1 
	   AND ca2.AuthorizationId IN (@COGED0001Id, @COGED0002Id)) AS CollaboratorsWithGED
FROM [auth].[ContactAuthorization] ca
WHERE ca.AccountId = -1
	AND ca.AuthorizationId = @COINVO001Id;

COMMIT TRANSACTION;
PRINT 'Script 0028 completed successfully';
GO
