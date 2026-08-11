-- =============================================
-- Script: Grant CLINVO001 to invoice portal contacts
-- Date: 2026-08-11
-- Description: Adds CLINVO001 permission to all invoice portal contacts (per account).
--              Contacts are extracted with 0029_Extract_invoice_portal_contacts.sql
--              on the account database.
-- =============================================

-- Contacts flagged ContactFlagPortailFactures in the account database
DROP TABLE IF EXISTS #InvoicePortalContacts;
CREATE TABLE #InvoicePortalContacts (AccountId INT NOT NULL, ContactId INT NOT NULL);

-- Replace the line below with the output of script 0029
INSERT INTO #InvoicePortalContacts (AccountId, ContactId)
VALUES
	(0, 0);

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

-- Add CLINVO001 to all invoice portal contacts (per account, not global)
INSERT INTO [auth].[ContactAuthorization] (ContactId, AccountId, AuthorizationId, CreationDate)
SELECT
	r.ContactId,
	r.AccountId,
	@AuthorizationId,
	GETUTCDATE()
FROM #InvoicePortalContacts ipc
INNER JOIN [account].[Role] r ON r.AccountId = ipc.AccountId AND r.ContactId = ipc.ContactId
INNER JOIN [actor].[Contact] c ON c.ContactId = r.ContactId
INNER JOIN [account].[Account] a ON a.AccountId = r.AccountId
WHERE c.Type = 'Customer'
	AND a.IsActive = 1
	AND NOT EXISTS (
		SELECT 1
		FROM [auth].[ContactAuthorization] ca
		WHERE ca.ContactId = r.ContactId
			AND ca.AccountId = r.AccountId
			AND ca.AuthorizationId = @AuthorizationId
	);

-- Show result
DECLARE @AddedCount INT = @@ROWCOUNT;
PRINT 'CLINVO001 permission added to ' + CAST(@AddedCount AS VARCHAR(10)) + ' invoice portal contacts';

-- Verify result
SELECT
	COUNT(ca.AuthorizationId) AS InvoicePortalContactsWithPermission,
	COUNT(*) AS TotalInvoicePortalContacts
FROM #InvoicePortalContacts ipc
LEFT JOIN [auth].[ContactAuthorization] ca
	ON ca.ContactId = ipc.ContactId
	AND ca.AccountId = ipc.AccountId
	AND ca.AuthorizationId = @AuthorizationId;

COMMIT TRANSACTION;
PRINT 'Script 0030 completed successfully';

DROP TABLE #InvoicePortalContacts;
GO
