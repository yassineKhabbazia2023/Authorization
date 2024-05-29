-- Clean authorization
DELETE FROM auth.AccountAuthorization
DELETE FROM auth.ContactAuthorization

-- Assign authorization for Account -1
INSERT INTO auth.AccountAuthorization(AccountId, AuthorizationId, Enabled)
SELECT
	-1,
	AuthorizationId,
	1
FROM auth.[Authorization]
where [Type] = 'collaborator'

-- Assign all authorization for all account
DECLARE @AccId int = 1
DECLARE @MaxAccId int
SELECT @MaxAccId = MAX(AccountId) from account.Account

WHILE @AccId <= @MaxAccId
BEGIN
	INSERT INTO auth.AccountAuthorization(AccountId, AuthorizationId, Enabled)
	SELECT
		@AccId,
		AuthorizationId,
		1
	FROM auth.[Authorization]
	SELECT @AccId = @AccId + 1
END


-- Assign authorization for all Collab
DECLARE @date DATETIME = GETDATE()
DECLARE @contactId int
DECLARE contactId_cursor CURSOR FOR
SELECT ContactId FROM actor.Contact Where Type = 'Collaborator'

OPEN contactId_cursor

FETCH NEXT FROM contactId_cursor
INTO @contactId

WHILE @@FETCH_STATUS = 0
BEGIN
	INSERT INTO auth.ContactAuthorization(ContactId, AccountId, AuthorizationId, CreationDate)
	SELECT
		@contactId,
		-1,
		AuthorizationId,
		@date
	FROM auth.[Authorization]
	where [Type] = 'collaborator'

	FETCH NEXT FROM contactId_cursor
	INTO @contactId
END

CLOSE contactId_cursor
DEALLOCATE contactId_cursor