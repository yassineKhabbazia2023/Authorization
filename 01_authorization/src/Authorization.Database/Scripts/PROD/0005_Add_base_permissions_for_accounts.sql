DECLARE @BasePermissions TABLE (AuthorizationId INT, Code VARCHAR(10))

INSERT INTO @BasePermissions (AuthorizationId, Code)
SELECT a.AuthorizationId, a.Code FROM [auth].[Authorization] a WHERE a.Code IN ('COUSER001', 'COUSER002', 'COOFF003', 'COINFO001')

SELECT DISTINCT ac.AccountId, aa.AuthorizationId
INTO #TempAffectedAccounts
FROM [account].[Account] ac
CROSS JOIN @BasePermissions p
JOIN [auth].[Authorization] aa ON aa.Code = p.Code
WHERE NOT EXISTS (
    SELECT 1
    FROM auth.AccountAuthorization aaa
    WHERE aaa.AccountId = ac.AccountId
    AND aaa.AuthorizationId = aa.AuthorizationId
);

-- VALUES WILL BE INSERTED INTO THE TABLE
SELECT * FROM #TempAffectedAccounts

-- INSERT THE VALUES
INSERT INTO auth.AccountAuthorization (AccountId, AuthorizationId)
SELECT AccountId, AuthorizationId
FROM #TempAffectedAccounts;

-- DELETE THE TEMPORARY TABLE
DROP TABLE #TempAffectedAccounts;