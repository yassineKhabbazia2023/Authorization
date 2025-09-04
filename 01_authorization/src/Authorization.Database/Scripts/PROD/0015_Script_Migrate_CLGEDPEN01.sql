BEGIN TRY
    BEGIN TRAN;

    -- Parameters
    DECLARE @ssoPennylaneCode  varchar(50) = 'CLPEN001';
    DECLARE @CLGEDPEN01        varchar(50) = 'CLGEDPEN01';

    -- Resolve AuthorizationIds once
    DECLARE @ssoId int, @penId int;

    SELECT @ssoId = AuthorizationId
    FROM [auth].[Authorization]
    WHERE Code = @ssoPennylaneCode;

    SELECT @penId = AuthorizationId
    FROM [auth].[Authorization]
    WHERE Code = @CLGEDPEN01;

    -- Capture inserted rows
    DECLARE @Inserted TABLE (
        ContactId      int,
        AccountId      int,
        AuthorizationId int,
        CreationDate   datetime
    );

    -- Insert the missing CLGEDPEN01 authorization and log inserted rows
    INSERT INTO auth.ContactAuthorization (ContactId, AccountId, AuthorizationId, CreationDate)
    OUTPUT inserted.ContactId, inserted.AccountId, inserted.AuthorizationId, inserted.CreationDate
    INTO @Inserted (ContactId, AccountId, AuthorizationId, CreationDate)
    SELECT DISTINCT ca.ContactId,
                    ca.AccountId,
                    @penId,
                    GETDATE()
    FROM auth.ContactAuthorization AS ca
    WHERE ca.AuthorizationId = @ssoId
      AND NOT EXISTS (
            SELECT 1
            FROM auth.ContactAuthorization AS x
            WHERE x.ContactId = ca.ContactId
              AND x.AccountId = ca.AccountId
              AND x.AuthorizationId = @penId
      );

    COMMIT TRAN;

    SELECT DISTINCT ContactId, AccountId
    FROM @Inserted
    ORDER BY ContactId;

END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRAN;

    THROW;
END CATCH;