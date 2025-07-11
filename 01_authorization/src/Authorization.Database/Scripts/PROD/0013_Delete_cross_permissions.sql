-- FIRST QUERY - Collaborators with customer permissions
-- Preview records to be deleted
SELECT ac.AuthorizationId, a.Code, a.Type AS AuthType, 
       c.ContactId, c.Email, c.Type AS UserType
FROM [auth].[ContactAuthorization] ac
INNER JOIN [auth].[Authorization] a ON ac.AuthorizationId = a.AuthorizationId
INNER JOIN [actor].[Contact] c ON ac.ContactId = c.ContactId
WHERE a.Type = 'customer' AND c.Type = 'Collaborator';

-- Delete operation
BEGIN TRANSACTION;

DELETE ac
FROM [auth].[ContactAuthorization] ac
INNER JOIN [auth].[Authorization] a ON ac.AuthorizationId = a.AuthorizationId
INNER JOIN [actor].[Contact] c ON ac.ContactId = c.ContactId
WHERE a.Type = 'customer' AND c.Type = 'Collaborator';

-- COMMIT TRANSACTION;
-- ROLLBACK TRANSACTION;

--

-- SECOND QUERY - Customers with collaborator permissions
-- Preview records to be deleted (inverted condition)
SELECT ac.AuthorizationId, a.Code, a.Type AS AuthType, 
       c.ContactId, c.Email, c.Type AS UserType
FROM [auth].[ContactAuthorization] ac
INNER JOIN [auth].[Authorization] a ON ac.AuthorizationId = a.AuthorizationId
INNER JOIN [actor].[Contact] c ON ac.ContactId = c.ContactId
WHERE a.Type = 'collaborator' AND c.Type = 'Customer';

-- Delete operation (inverted condition)
BEGIN TRANSACTION;

DELETE ac
FROM [auth].[ContactAuthorization] ac
INNER JOIN [auth].[Authorization] a ON ac.AuthorizationId = a.AuthorizationId
INNER JOIN [actor].[Contact] c ON ac.ContactId = c.ContactId
WHERE a.Type = 'collaborator' AND c.Type = 'Customer';

-- COMMIT TRANSACTION;
-- ROLLBACK TRANSACTION;
