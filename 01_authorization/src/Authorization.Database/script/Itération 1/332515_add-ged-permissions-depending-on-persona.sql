DECLARE @COGED0001 VARCHAR(20)
DECLARE @COGED0002 VARCHAR(20)

SET @COGED0001 = (SELECT a.AuthorizationId FROM [auth].[Authorization] a WHERE a.Code = 'COGED0001')
SET @COGED0002 = (SELECT a.AuthorizationId FROM [auth].[Authorization] a WHERE a.Code = 'COGED0002')

-- ATTRIBUTION DE LA PERMISSION COGED0001
INSERT INTO [auth].[ContactAuthorization] (ContactId, AccountId, AuthorizationId, CreationDate)
SELECT ca.ContactId, -1, @COGED0001, GETDATE()
FROM [actor].[Contact] ca
WHERE ca.PersonaName = 'Collaborateur GS' AND Type = 'Collaborator'
	AND NOT EXISTS (
		SELECT 1
		FROM [auth].[ContactAuthorization] ac
		WHERE ac.ContactId = ca.ContactId
		AND ac.AuthorizationId = @COGED0001
	)

-- ATTRIBUTION DE LA PERMISSION COGED0002
INSERT INTO [auth].[ContactAuthorization] (ContactId, AccountId, AuthorizationId, CreationDate)
SELECT ca.ContactId, -1, @COGED0002, GETDATE()
FROM [actor].[Contact] ca
WHERE ca.PersonaName = 'Collaborateur ESC' AND Type = 'Collaborator'
	AND NOT EXISTS (
		SELECT 1
		FROM [auth].[ContactAuthorization] ac
		WHERE ac.ContactId = ca.ContactId
		AND ac.AuthorizationId = @COGED0002
	)

