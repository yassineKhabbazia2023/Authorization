IF (SELECT count(*) FROM [auth].[Authorization] WHERE Code = 'CORAP003') = 0
BEGIN
INSERT INTO auth.[Authorization] (
	Name, Description, Code, Label, Category, Configurable, [View], [ProductCode], [Type])
	VALUES
	('Delete report', '', 'CORAP003', 'Supprimer un rapport BI', 'COLRAPPORT', '1', 'Global', NULL, 'collaborator')
END

DECLARE @CORAP003 INT;
SELECT @CORAP003 = AuthorizationId FROM [auth].[Authorization] WHERE Code = 'CORAP003'

IF (SELECT count(*) FROM [auth].[AccountAuthorization] WHERE AuthorizationId = @CORAP003) = 0
BEGIN
	INSERT INTO [auth].[AccountAuthorization]
	VALUES(-1, @CORAP003, 1)
END
 
