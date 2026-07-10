-- Script de rattrapage: Ajout des permissions Prospect aux comptes Prospect existants
-- Date: 2026-07-09
-- Description: Ajoute les permissions CLPCONF* et CLPDOCP* aux comptes avec AccountType = 'Prospect'

BEGIN TRANSACTION;

DECLARE @ProspectPermissionCodes TABLE (Code NVARCHAR(50));

-- Codes permissions Prospect à ajouter
INSERT INTO @ProspectPermissionCodes (Code) VALUES
    ('CLPCONF001'),
    ('CLPCONF002'),
    ('CLPCONF004'),
    ('CLPCONF005'),
    ('CLPCONF006'),
    ('CLPDOCP001');

-- Insertion permissions manquantes pour chaque compte Prospect
INSERT INTO [auth].[AccountAuthorization] (AccountId, AuthorizationId, Enabled)
SELECT DISTINCT
    a.AccountId,
    auth.AuthorizationId,
    1 AS Enabled
FROM [account].[Account] a
CROSS JOIN [auth].[Authorization] auth
INNER JOIN @ProspectPermissionCodes ppc ON auth.Code = ppc.Code
WHERE a.AccountType = 'Prospect'
    AND a.IsActive = 1
    AND NOT EXISTS (
        SELECT 1
        FROM [auth].[AccountAuthorization] aa
        WHERE aa.AccountId = a.AccountId
            AND aa.AuthorizationId = auth.AuthorizationId
    );

-- Vérification résultat
SELECT
    a.AccountId,
    a.LegalName,
    a.AccountType,
    COUNT(aa.AuthorizationId) AS TotalPermissions,
    COUNT(CASE WHEN auth.TargetAccountType = 'Prospect' THEN 1 END) AS ProspectPermissions
FROM [account].[Account] a
LEFT JOIN [auth].[AccountAuthorization] aa ON a.AccountId = aa.AccountId
LEFT JOIN [auth].[Authorization] auth ON aa.AuthorizationId = auth.AuthorizationId
WHERE a.AccountType = 'Prospect'
    AND a.IsActive = 1
GROUP BY a.AccountId, a.LegalName, a.AccountType
ORDER BY a.AccountId;

-- Décommenter pour valider transaction
-- COMMIT TRANSACTION;

-- Décommenter pour annuler (test)
ROLLBACK TRANSACTION;
