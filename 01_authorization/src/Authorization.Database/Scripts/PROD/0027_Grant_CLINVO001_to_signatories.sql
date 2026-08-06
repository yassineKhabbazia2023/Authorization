-- =============================================
-- Script: Grant CLINVO001 to all existing signatories
-- Date: 2026-07-10
-- Description: Adds CLINVO001 permission to all existing signatories (per account)
-- =============================================

BEGIN TRANSACTION;

-- Get CLINVO001 permission ID
DECLARE @AuthorizationId INT;
SELECT @AuthorizationId = AuthorizationId
FROM [auth].[Authorization]
WHERE Code = 'CLINVO001';

-- Check permission exists
IF @AuthorizationId IS NULL
BEGIN
	PRINT 'ERROR: Permission CLINVO001 not found. Run 0026_Update_CLINVO001_COINVO001_permissions.sql first.';
	ROLLBACK TRANSACTION;
	RETURN;
END

-- Add CLINVO001 to all signatories (per account, not global)
INSERT INTO [auth].[ContactAuthorization] (ContactId, AccountId, AuthorizationId, CreationDate)
SELECT
	r.ContactId,
	r.AccountId,
	@AuthorizationId,
	GETUTCDATE()
FROM [account].[Role] r
INNER JOIN [account].[Account] a ON r.AccountId = a.AccountId
WHERE r.IsSignatory = 1
	AND a.AccountType = 'Client'
	AND NOT EXISTS (
		SELECT 1
		FROM [auth].[ContactAuthorization] ca
		WHERE ca.ContactId = r.ContactId
			AND ca.AccountId = r.AccountId
			AND ca.AuthorizationId = @AuthorizationId
	);

-- Show result
DECLARE @AddedCount INT = @@ROWCOUNT;
PRINT 'CLINVO001 permission added to ' + CAST(@AddedCount AS VARCHAR(10)) + ' signatories';

-- Verify result
SELECT
	COUNT(DISTINCT ca.ContactId) AS SignatoriesWithPermission,
	(SELECT COUNT(DISTINCT r2.ContactId) 
	 FROM [account].[Role] r2 
	 INNER JOIN [account].[Account] a2 ON r2.AccountId = a2.AccountId
	 WHERE r2.IsSignatory = 1 AND a2.AccountType = 'Client') AS TotalSignatories
FROM [account].[Role] r
INNER JOIN [account].[Account] a ON r.AccountId = a.AccountId
LEFT JOIN [auth].[ContactAuthorization] ca
	ON r.ContactId = ca.ContactId
	AND r.AccountId = ca.AccountId
	AND ca.AuthorizationId = @AuthorizationId
WHERE r.IsSignatory = 1
	AND a.AccountType = 'Client';

COMMIT TRANSACTION;
PRINT 'Script 0027 completed successfully';
GO
