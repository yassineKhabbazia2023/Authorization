IF (SELECT count(*) FROM [auth].[Authorization] WHERE Code = 'COADMI006') = 0
BEGIN
INSERT INTO auth.[Authorization] (
	Name, Description, Code, Label, Category, Configurable, [View], [ProductCode], [Type])
	VALUES
	('Add User', '', 'COADMI006', 'Ajouter un utilisateur', 'COLADMIN', '1', 'Global', NULL, 'collaborator')
END

DECLARE @COADMI006 INT;
SELECT @COADMI006 = AuthorizationId FROM [auth].[Authorization] WHERE Code = 'COADMI006'

IF (SELECT count(*) FROM [auth].[AccountAuthorization] WHERE AuthorizationId = @COADMI006) = 0
BEGIN
	INSERT INTO [auth].[AccountAuthorization]
	VALUES(-1, @COADMI006, 1)
END
 
