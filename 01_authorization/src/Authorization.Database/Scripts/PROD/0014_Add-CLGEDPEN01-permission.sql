IF (SELECT count(*) FROM [auth].[Authorization] WHERE Code = 'CLGEDPEN01') = 0
BEGIN
INSERT INTO auth.[Authorization] (
	Name, Description, Code, Label, Category, Configurable, [View], [ProductCode], [Type])
	VALUES
	('Access Rydge Conseil', '', 'CLGEDPEN01', 'Accéder à l’espace documentaire Rydge Conseil', 'CLTGEDESC', '1', 'Partial', 'pennylaneaccess', 'customer')
END

-- Target: add CLGEDPEN01 to accounts that already have CLPEN001

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRAN;

    -- Resolve AuthorizationIds
    DECLARE @CLGEDPEN01 INT; -- "Gérer les documents Pennylane" (example)
    SELECT @CLGEDPEN01 = AuthorizationId
    FROM [auth].[Authorization]
    WHERE Code = 'CLGEDPEN01';

    DECLARE @CLPEN001 INT;   -- "Accéder à l'outil Pennylane"
    SELECT @CLPEN001 = AuthorizationId
    FROM [auth].[Authorization]
    WHERE Code = 'CLPEN001';

    IF @CLGEDPEN01 IS NULL
        THROW 50001, 'Authorization code CLGEDPEN01 not found in auth.Authorization.', 1;

    IF @CLPEN001 IS NULL
        THROW 50002, 'Authorization code CLPEN001 not found in auth.Authorization.', 1;

    -- Capture inserted AccountIds for reporting
    DECLARE @InsertedAccounts TABLE (AccountId INT PRIMARY KEY);

    INSERT INTO auth.AccountAuthorization (AccountId, AuthorizationId, Enabled)
    OUTPUT inserted.AccountId INTO @InsertedAccounts(AccountId)
    SELECT DISTINCT aa.AccountId, @CLGEDPEN01 AS AuthorizationId, 1 AS Enabled
    FROM auth.AccountAuthorization AS aa
    WHERE aa.AuthorizationId = @CLPEN001
      AND NOT EXISTS (
            SELECT 1
            FROM auth.AccountAuthorization AS existsAA
            WHERE existsAA.AccountId = aa.AccountId
              AND existsAA.AuthorizationId = @CLGEDPEN01
      );

    COMMIT TRAN;

    -- Report
    SELECT COUNT(*) AS InsertedCount FROM @InsertedAccounts;
    SELECT AccountId FROM @InsertedAccounts ORDER BY AccountId;

END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRAN;

    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(),
            @ErrNum INT = ERROR_NUMBER(),
            @ErrState INT = ERROR_STATE(),
            @ErrSeverity INT = ERROR_SEVERITY(),
            @ErrLine INT = ERROR_LINE();

    RAISERROR('Failed to add CLGEDPEN01 to accounts with CLPEN001. [%d] %s (Severity %d, State %d, Line %d)',
              @ErrSeverity, 1, @ErrNum, @ErrMsg, @ErrSeverity, @ErrState, @ErrLine);
END CATCH;
