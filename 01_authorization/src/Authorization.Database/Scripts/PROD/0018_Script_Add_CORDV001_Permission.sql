IF (SELECT count(*) FROM [auth].[Authorization] WHERE Code = 'CORDV001') = 0
BEGIN
	INSERT INTO [auth].[Authorization] (
		[Name], [Description], [Code], [Label], [Category], [Configurable], [View], [ProductCode], [Type])
		VALUES
		('Manage booking page', '', 'CORDV001', 'Gérer ma page de rendez-vous avec booking', 'COLGESTION', '1', 'Global', NULL, 'collaborator')
END

DECLARE @CORDV001 INT;
SELECT @CORDV001 = AuthorizationId FROM [auth].[Authorization] WHERE Code = 'CORDV001'

IF (SELECT count(*) FROM [auth].[AccountAuthorization] WHERE AuthorizationId = @CORDV001) = 0
BEGIN
	INSERT INTO [auth].[AccountAuthorization] (AccountId, AuthorizationId, Enabled)
	VALUES(-1, @CORDV001, 1)
END
