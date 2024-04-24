IF (SELECT count(*) FROM [auth].[Persona]) = 0
BEGIN
SET IDENTITY_INSERT [auth].[Persona] ON; 
	INSERT [auth].[Persona] ([PersonaId], [Name], [Description], [Type]) VALUES 
		(1, N'ADMIN / Dirigeant', N'', 2),
		(2, N'ADMIN / Associé', N'', 2),
		(3, N'Assistante', N'', 2),
		(4, N'Responsable financier', N'', 2),
		(5, N'RH', N'', 2),
		(6, N'Comptable', N'', 2),
		(7, N'Support / CX', N'', 1),
		(8, N'Admin / Collab Product', N'', 1),
		(9, N'Partner ESC', N'', 1),
		(10, N'Partner GS', N'', 1),
		(11, N'Collaborateur ESC', N'', 1),
		(12, N'Collaborateur GS', N'', 1),
		(13, N'Assitante GS', N'', 1),
		(14, N'Assitante ESC', N'', 1),
		(15, N'Expert Digital ESC', N'', 1),
		(16, N'Expert Digital GS', N'', 1),
		(17, N'MOD ESC', N'', 1),
		(18, N'MOD GS', N'', 1),
		(19, N'MOD Forms', N'', 1),
		(20, N'Collaborateur BI', N'', 1),
		(21, N'Collaborateur', N'', 1),
		(22, N'Customer', N'', 2)
SET IDENTITY_INSERT [auth].[Persona] OFF;
END