IF (SELECT count(*) FROM [auth].[Resource]) = 0
BEGIN
SET IDENTITY_INSERT [auth].[Resource] ON; 

	INSERT INTO [auth].[Resource](ResourceId,ParentId, Name, Label, Category, Type, Code, OrderShow) VALUES
		-- Vue Collaborateurs
	(1, NULL,'mandat','Mandat', 'globale', 'Collaborator', 'MCOL0001', 1),
	(2, NULL,'mod','Demandes MOD', 'globale','Collaborator', 'MCOL0002', 2),
	(3, NULL,'collaborators','Collaborateurs', 'globale', 'Collaborator', 'MCOL0003', 3),
	(4, NULL,'history','Historique', 'globale', 'Collaborator', 'MCOL0004', 4),
	(5, NULL,'entity','Dossier', 'unitaire', 'Collaborator', 'MCOL0005', 1),
		-- Insert  entity childs
		(6, 5,'users','Utilisateurs', 'unitaire','Collaborator', 'MCOL5006', 2),
		(7, 5,'collaborators','Collaborateurs', 'unitaire','Collaborator', 'MCOL5007', 3),
		(8, 5,'offers','Offres', 'unitaire', 'Collaborator', 'MCOL5008', 4),
		(9, 5,'informations','Informations', 'unitaire', 'Collaborator', 'MCOL5009', 5),
	(10, NULL,'documents','Documents', 'unitaire', 'Collaborator', 'MCOL0019', 6),
	(11, NULL,'bank','Banque', 'unitaire', 'Collaborator', 'MCOL0011', 7),
	(12, NULL,'indicators','Indicateurs', 'unitaire', 'Collaborator', 'MCOL0012', 8),
	-- Vue Client
	(13, NULL,'entities','Mes entités', 'globale', 'Customer', 'MCUS0001', 1),
	(14, NULL,'Users','Tous les utilisateurs', 'globale','Customer', 'MCUS0002', 2),
	(15, NULL,'entity','Dossier', 'unitaire', 'Customer', 'MCUS0003', 1),
		-- Insert  entity childs
		(16, 15,'users','Utilisateurs', 'unitaire','Customer', 'MCUS3004', 2),
		(17, 15,'collaborators','Collaborateurs KPMG', 'unitaire','Customer', 'MCUS3005', 3),
		(18, 15,'informations','Informations', 'unitaire', 'Customer', 'MCUS3006', 5),
	(19, NULL,'billing','Factures KPMG', 'unitaire', 'Customer', 'MCUS0007', 6)

SET IDENTITY_INSERT [auth].[Resource] OFF; 
END