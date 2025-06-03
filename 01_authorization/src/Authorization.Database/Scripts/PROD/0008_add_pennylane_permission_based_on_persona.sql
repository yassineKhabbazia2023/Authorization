DECLARE @COPEN001 VARCHAR(20)
DECLARE @COGED0002 VARCHAR(20)

SET @COPEN001 = (SELECT a.AuthorizationId FROM [auth].[Authorization] a WHERE a.Code = 'COPEN001')
SET @COGED0002 = (SELECT a.AuthorizationId FROM [auth].[Authorization] a WHERE a.Code = 'COGED0002')

-- ATTRIBUTION DE LA PERMISSION COPEN001
INSERT INTO 
	[auth].[ContactAuthorization] (ContactId, AccountId, AuthorizationId, CreationDate)
SELECT 
	ca.ContactId,
	-1,
	@COPEN001,
	GETDATE()
FROM 
	[actor].[Contact] ca
LEFT JOIN 
	[auth].[ContactAuthorization] aca
ON 
	ca.contactId = aca.contactId
WHERE 
	Type = 'Collaborator' 
	AND 
		ca.PersonaName = 'Collaborateur ESC' 
	OR 
		aca.AuthorizationId = @COGED0002
  	AND 
		NOT EXISTS (
    		SELECT 
					1
    		FROM
					[auth].[ContactAuthorization] ac
    		WHERE 
					ac.ContactId = ca.ContactId
    		AND 
					ac.AuthorizationId = @COPEN001
    		AND 
					ac.AccountId = -1
  			)