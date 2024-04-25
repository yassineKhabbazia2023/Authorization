IF (SELECT count(*) FROM [account].[Account] WHERE [AccountId] = -1) = 0
BEGIN
SET IDENTITY_INSERT [account].[Account] ON; 

	INSERT INTO [account].[Account] ([AccountId], [AccountGlobalUniqueId], [AccountNumber], [LegalName], [Status]) VALUES (-1, N'e45770e9-db63-46e1-b588-461569a80f9f', N'11111111', N'Menu-globale-collab', N'ToDeploy')

SET IDENTITY_INSERT [account].[Account] OFF; 
END