IF (SELECT count(*) FROM [auth].[Authorization] WHERE Code = 'COAIOFFER') = 0
BEGIN
INSERT INTO auth.[Authorization] (
	Name, Description, Code, Label, Category, Configurable, [View], [ProductCode], [Type])
	VALUES
	('Access AI Mode', '', 'COAIOFFER', 'Accéder au mode IA du catalogue des offres', 'COLOFFRE', '1', 'Global', NULL, 'collaborator')
END

DECLARE @COAIOFFER INT;
SELECT @COAIOFFER = AuthorizationId FROM [auth].[Authorization] WHERE Code = 'COAIOFFER'

IF (SELECT count(*) FROM [auth].[AccountAuthorization] WHERE AuthorizationId = @COAIOFFER) = 0
BEGIN
	INSERT INTO [auth].[AccountAuthorization]
	VALUES(-1, @COAIOFFER, 1)
END
 
