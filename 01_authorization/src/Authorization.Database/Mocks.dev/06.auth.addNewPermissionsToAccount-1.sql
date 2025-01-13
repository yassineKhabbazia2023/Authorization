DECLARE @PermissionCodeList TABLE (Code VARCHAR(50));
INSERT INTO @PermissionCodeList (Code)
VALUES
    ('COADMI004'),
    ('COADMI005'),
    ('COADMI007'),
    ('COINFO002');

DECLARE @AuthIDsToInsert TABLE (ID INT PRIMARY KEY);

INSERT INTO @AuthIDsToInsert (ID)
SELECT auth.AuthorizationId
FROM [auth].[Authorization] auth
INNER JOIN @PermissionCodeList pcl ON auth.Code = pcl.Code
WHERE NOT EXISTS (
    SELECT 1
    FROM [auth].[AccountAuthorization] accountAuth
    WHERE accountAuth.AuthorizationId = auth.AuthorizationId
);

INSERT INTO [auth].[AccountAuthorization] (AccountId, AuthorizationId, Enabled)
SELECT -1, ID, 1
FROM @AuthIDsToInsert;
