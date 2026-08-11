-- =============================================
-- Script: Extract invoice portal contacts
-- Date: 2026-08-11
-- Target database: account (NOT authorization)
-- Description: Extracts (AccountId, ContactId) pairs of invoice portal contacts,
--              to be pasted into 0030_Grant_CLINVO001_to_invoice_portal_contacts.sql
-- =============================================

-- Detailed list (review before running script 0030)
SELECT
	r.AccountId,
	r.ContactId,
	a.AccountNumber,
	a.LegalName,
	c.Email,
	r.IsSignatory
FROM [account].[Role] r
INNER JOIN [actor].[Contact] c ON c.ContactId = r.ContactId
INNER JOIN [account].[Account] a ON a.AccountId = r.AccountId
WHERE r.ContactFlagPortailFactures = 1
	AND c.Type = 'Customer'
ORDER BY r.AccountId, r.ContactId;

-- VALUES block to paste into script 0030
SELECT STRING_AGG(
	CAST('(' + CAST(x.AccountId AS VARCHAR(20)) + ', ' + CAST(x.ContactId AS VARCHAR(20)) + ')' AS VARCHAR(MAX)),
	',' + CHAR(13) + CHAR(10)) AS ValuesToPaste
FROM (
	SELECT r.AccountId, r.ContactId
	FROM [account].[Role] r
	INNER JOIN [actor].[Contact] c ON c.ContactId = r.ContactId
	WHERE r.ContactFlagPortailFactures = 1
		AND c.Type = 'Customer'
) x;
