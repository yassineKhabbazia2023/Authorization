IF (SELECT count(*) FROM [auth].[Persona]) = 0
BEGIN
	INSERT [auth].[Persona] ([PersonaId], [Name], [Description], [Type]) VALUES 
		(1, 'Administrateur', '', 'Collaborator'),
		(2, 'Assistant', '', 'Collaborator'),
		(3, 'Autres collaborateurs', '', 'Collaborator'),
		(4, 'Client', '', 'Customer'),
		(5, 'Collaborateur ESC', '', 'Collaborator'),
		(6, 'Collaborateur GS', '', 'Collaborator'),
		(7, 'Digital team', '', 'Collaborator'),
		(8, 'Directeur', '', 'Collaborator'),
		(9, 'Equipe BI', '', 'Collaborator'),
		(10, 'Expert Digital', '', 'Collaborator'),
		(11, 'Middle office', '', 'Collaborator'),
		(12, 'Partner', '', 'Collaborator'),
		(13, 'Support', '', 'Collaborator'),
		(14, 'Administrateur', '', 'Customer')
END