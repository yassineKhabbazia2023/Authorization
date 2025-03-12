DECLARE @MirrorPermissionsToCheck TABLE (AuthorizationId INT, Code VARCHAR(10))
DECLARE @MirrorPermissionsToAdd TABLE (AuthorizationId INT, Code VARCHAR(10))

-- INSERTION DANS LA TABLE @MirrorPermissionsToCheck AVEC LES PERMISSIONS DONT NOUS SOUHAITONS VÉRIFIER L’EXISTENCE,
-- CE QUI EVITE QUE LES PERMISSIONS NE SOIENT PAS STOCKEES AVEC LE MÊME ID DANS LES DIFFERENTES BASES
INSERT INTO @MirrorPermissionsToCheck (AuthorizationId, Code)
SELECT a.AuthorizationId, a.Code FROM [auth].[Authorization] a WHERE a.Code IN ('COUSER001', 'COUSER002', 'COOFF003', 'COINFO001', 'COLANC001')

-- INSERTION DANS LA TABLE @MirrorPermissionsToAdd AVEC LES PERMISSIONS QUE NOUS SOUHAITONS AJOUTER
INSERT INTO @MirrorPermissionsToAdd (AuthorizationId, Code)
SELECT a.AuthorizationId, a.Code FROM [auth].[Authorization] a WHERE a.Code IN ('COUSER001', 'COUSER002', 'COOFF003', 'COINFO001', 'COLANC001', 'COCAL001', 'COGES001', 'COMAND001', 'COINFO002')

-- DECLARATION D'UNE VARIABLE CONTACTID POUR STOCKER LE CONTACTID AU NIVEAU DE LA BOUCLE
DECLARE @ContactID INT

-- DECLARATION D'UN CURSEUR POUR RECUPERER NOS CONTACTS SANS LES PERMISSIONS DE LA VUE MIROIR
DECLARE contact_cursor CURSOR FOR
	SELECT DISTINCT ac.ContactId
	FROM [actor].[Contact] ac
	WHERE NOT EXISTS (
		SELECT 1
		FROM auth.ContactAuthorization auc
		JOIN [auth].[Authorization] aa ON auc.AuthorizationId = aa.AuthorizationId
		WHERE auc.ContactId = ac.ContactId
		AND aa.Code IN (SELECT p.Code FROM @MirrorPermissionsToCheck p)
	) AND ac.Type = 'Collaborator'

-- OUVERTURE DU CURSOR
OPEN contact_cursor;

-- RECUPERER LE PREMIER CONTACT POUR COMMENCER LA BOUCLE
FETCH NEXT FROM contact_cursor INTO @ContactID

-- BOUCLER SUR TOUS LES CONTACTS SANS LES PERMISSIONS DE LA VUE MIROIR
WHILE @@FETCH_STATUS = 0
BEGIN
	INSERT INTO [auth].[ContactAuthorization] (ContactId, AccountId, AuthorizationId, CreationDate)
	SELECT @ContactID, -1, p.AuthorizationId, GETDATE()
	FROM @MirrorPermissionsToAdd P
	WHERE NOT EXISTS (
		SELECT 1
		FROM [auth].[ContactAuthorization] ac
		WHERE ac.ContactId = @ContactID
		AND ac.AuthorizationId = p.AuthorizationId
	)
	-- RECUPERER LE CONTACT SUIVANT
	FETCH NEXT FROM contact_cursor INTO @ContactId;
END

-- FERMER ET LIBERER LE CURSEUR
CLOSE contact_cursor;
DEALLOCATE contact_cursor;
