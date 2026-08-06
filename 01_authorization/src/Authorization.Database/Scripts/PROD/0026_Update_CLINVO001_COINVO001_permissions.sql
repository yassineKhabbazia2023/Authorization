-- =============================================
-- Script: Update CLINVO001 and COINVO001 permissions
-- Date: 2026-07-10
-- Description: Update permissions to Configurable = 1 and add to existing Client accounts
-- =============================================

BEGIN TRANSACTION;

-- Update CLINVO001: Only Configurable changes (0 -> 1)
UPDATE [auth].[Authorization]
SET [Label] = 'Accéder aux factures Rydge Conseil',
	[Configurable] = 1
WHERE Code = 'CLINVO001';

IF @@ROWCOUNT > 0
	PRINT 'CLINVO001 updated: Configurable = 1';
ELSE
	PRINT 'WARNING: CLINVO001 not found';

-- Update COINVO001: Label, View and Configurable change
UPDATE [auth].[Authorization]
SET [Label] = 'Accéder aux factures Rydge Conseil',
	[View] = 'Global',
	[Configurable] = 1
WHERE Code = 'COINVO001';

IF @@ROWCOUNT > 0
	PRINT 'COINVO001 updated: Label, View = Global, Configurable = 1';
ELSE
	PRINT 'WARNING: COINVO001 not found';

-- Get permission IDs
DECLARE @CLINVO001Id INT;
DECLARE @COINVO001Id INT;

SELECT @CLINVO001Id = AuthorizationId FROM [auth].[Authorization] WHERE Code = 'CLINVO001';
SELECT @COINVO001Id = AuthorizationId FROM [auth].[Authorization] WHERE Code = 'COINVO001';

-- Check permissions exist
IF @CLINVO001Id IS NULL OR @COINVO001Id IS NULL
BEGIN
	PRINT 'ERROR: One or both permissions not found. Rolling back.';
	ROLLBACK TRANSACTION;
	RETURN;
END

-- Add CLINVO001 to all existing Client accounts (not Prospect)
INSERT INTO [auth].[AccountAuthorization] (AccountId, AuthorizationId, Enabled)
SELECT
	a.AccountId,
	@CLINVO001Id,
	1
FROM [account].[Account] a
WHERE a.AccountType = 'Client'
	AND NOT EXISTS (
		SELECT 1
		FROM [auth].[AccountAuthorization] aa
		WHERE aa.AccountId = a.AccountId
			AND aa.AuthorizationId = @CLINVO001Id
	);

DECLARE @CLINVO001Count INT = @@ROWCOUNT;
PRINT 'CLINVO001 added to ' + CAST(@CLINVO001Count AS VARCHAR(10)) + ' Client accounts';

-- Add COINVO001 to all existing Client accounts (not Prospect)
INSERT INTO [auth].[AccountAuthorization] (AccountId, AuthorizationId, Enabled)
SELECT
	a.AccountId,
	@COINVO001Id,
	1
FROM [account].[Account] a
WHERE a.AccountType = 'Client'
	AND NOT EXISTS (
		SELECT 1
		FROM [auth].[AccountAuthorization] aa
		WHERE aa.AccountId = a.AccountId
			AND aa.AuthorizationId = @COINVO001Id
	);

DECLARE @COINVO001Count INT = @@ROWCOUNT;
PRINT 'COINVO001 added to ' + CAST(@COINVO001Count AS VARCHAR(10)) + ' Client accounts';

COMMIT TRANSACTION;
PRINT 'Script 0026 completed successfully';
GO
