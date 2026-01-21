IF (SELECT count(*) FROM [auth].[Authorization] WHERE Code = 'CORAP002') = 0
BEGIN
	INSERT INTO [auth].[Authorization] (
		[Name], [Description], [Code], [Label], [Category], [Configurable], [View], [ProductCode], [Type])
		VALUES
		('Add report', '', 'CORAP002', 'Déposer un rapport BI', 'COLRAPPORT', '1', 'Global', NULL, 'collaborator')

	UPDATE [auth].[Authorization]
	SET [Name] = 'Report administration',
	[Label] = 'Administrer les rapports BI'
	WHERE Code = 'CORAP001'

	DECLARE @newAutId INT;
	SET @newAutId = (SELECT [AuthorizationId] FROM [auth].[Authorization] WHERE Code = 'CORAP002');
	INSERT INTO [auth].[AccountAuthorization] ([AccountId] ,[AuthorizationId] ,[Enabled])
     VALUES (-1 ,@newAutId ,1)
END