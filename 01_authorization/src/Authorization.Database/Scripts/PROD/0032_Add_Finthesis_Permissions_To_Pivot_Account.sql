-- =============================================
-- Script: Add Finthesis collaborator permissions to pivot account
-- Date: 2026-08-24
-- Description: Attach COFIN001 and COFINM001 to Account -1 (pivot account)
--              so they appear in the collaborator settings screen.
--              Requires 0031_Add_Finthesis_Permissions.sql to run first.
-- =============================================

-- COFIN001 -> Account -1
DECLARE @COFIN001Id INT;
SELECT @COFIN001Id = AuthorizationId
FROM [auth].[Authorization]
WHERE Code = 'COFIN001';

IF @COFIN001Id IS NOT NULL AND NOT EXISTS (
    SELECT 1
    FROM [auth].[AccountAuthorization]
    WHERE AccountId = -1 AND AuthorizationId = @COFIN001Id
)
BEGIN
    INSERT INTO [auth].[AccountAuthorization] (AccountId, AuthorizationId, Enabled)
    VALUES (-1, @COFIN001Id, 1);
    PRINT 'COFIN001 added to Account -1';
END
ELSE IF @COFIN001Id IS NOT NULL
BEGIN
    PRINT 'COFIN001 already exists for Account -1';
END
GO

-- COFINM001 -> Account -1
DECLARE @COFINM001Id INT;
SELECT @COFINM001Id = AuthorizationId
FROM [auth].[Authorization]
WHERE Code = 'COFINM001';

IF @COFINM001Id IS NOT NULL AND NOT EXISTS (
    SELECT 1
    FROM [auth].[AccountAuthorization]
    WHERE AccountId = -1 AND AuthorizationId = @COFINM001Id
)
BEGIN
    INSERT INTO [auth].[AccountAuthorization] (AccountId, AuthorizationId, Enabled)
    VALUES (-1, @COFINM001Id, 1);
    PRINT 'COFINM001 added to Account -1';
END
ELSE IF @COFINM001Id IS NOT NULL
BEGIN
    PRINT 'COFINM001 already exists for Account -1';
END
GO
