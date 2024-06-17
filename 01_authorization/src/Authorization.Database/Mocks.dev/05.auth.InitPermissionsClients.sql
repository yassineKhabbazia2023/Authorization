-- Assign authorization for clients
DECLARE @clientId int
DECLARE clientId_cursor CURSOR FOR
SELECT ContactId FROM actor.Contact Where ContactId >= 74 AND ContactId <= 79

OPEN clientId_cursor

FETCH NEXT FROM clientId_cursor
INTO @clientId

WHILE @@FETCH_STATUS = 0
BEGIN
	DECLARE @accountId int
	DECLARE accountId_cursor CURSOR FOR
	SELECT AccountId FROM account.Role WHERE ContactId = @clientId

	OPEN accountId_cursor

	FETCH NEXT FROM accountId_cursor
	INTO @accountId

	WHILE @@FETCH_STATUS = 0
	BEGIN
		INSERT INTO auth.ContactAuthorization(ContactId, AccountId, AuthorizationId, CreationDate)
		SELECT
			@clientId,
			@accountId,
			AuthorizationId,
			GETDATE()
		FROM auth.[Authorization]

		FETCH NEXT FROM accountId_cursor
		INTO @accountId
	END

	CLOSE accountId_cursor
	DEALLOCATE accountId_cursor

	FETCH NEXT FROM clientId_cursor
	INTO @clientId
END


CLOSE clientId_cursor
DEALLOCATE clientId_cursor
