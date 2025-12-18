UPDATE [auth].[Authorization]
SET [View] = 'Partial'
WHERE [View] = 'Both'
  AND [Code] = 'CLUSER004';
GO
DELETE FROM auth.AccountAuthorization
WHERE AccountId = -1
  AND AuthorizationId = (
        SELECT AuthorizationId
        FROM [auth].[Authorization]
        WHERE Code = 'CLUSER004'
  );
GO