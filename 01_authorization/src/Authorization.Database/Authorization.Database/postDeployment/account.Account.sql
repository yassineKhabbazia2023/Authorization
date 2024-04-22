IF (SELECT count(*) FROM [account].[Account]) = 0
BEGIN
SET IDENTITY_INSERT [account].[Account] ON; 

	INSERT INTO [account].[Account] ([AccountId], [AccountGlobalUniqueId], [AccountNumber], [LegalName], [Status]) VALUES (-1, N'e45770e9-db63-46e1-b588-461569a80f9f', N'11111111', N'Menu-globale-collab', N'ToDeploy')
	INSERT INTO [account].[Account] ([AccountId], [AccountGlobalUniqueId], [AccountNumber], [LegalName], [Status]) VALUES (0, N'313e6772-b2b7-4d90-9aaa-d1c4ba3176c7', N'22222222', N'Menu-globale-customer', N'ToDeploy')

SET IDENTITY_INSERT [account].[Account] OFF; 
END