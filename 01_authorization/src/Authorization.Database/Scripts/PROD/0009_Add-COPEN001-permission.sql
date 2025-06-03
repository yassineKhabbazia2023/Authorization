IF (SELECT count(*) FROM [auth].[Authorization] WHERE Code = 'COPEN001') = 0
BEGIN
INSERT INTO auth.[Authorization] (
	Name, Description, Code, Label, Category, Configurable, [View], [ProductCode], [Type])
	VALUES
	('Access Pennylane collab', '', 'COPEN001', 'Accéder au lanceur pennylane', 'COLGESTION', '1', 'Global', NULL, 'collaborator')
END

DECLARE @COPEN001 INT;
SELECT @COPEN001 = AuthorizationId FROM [auth].[Authorization] WHERE Code = 'COPEN001'

IF (SELECT count(*) FROM [auth].[AccountAuthorization] WHERE AuthorizationId = @COPEN001) = 0
BEGIN
	INSERT INTO [auth].[AccountAuthorization]
	VALUES(-1, @COPEN001, 1)
END
 
