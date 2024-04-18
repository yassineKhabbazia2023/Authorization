IF (SELECT count(*) FROM [auth].[Resource]) = 0
BEGIN
SET IDENTITY_INSERT [auth].[Resource] OFF; 

	INSERT INTO [auth].[Resource](ResourceId,ParentId, Name, Label, Category, Code, OrderShow) VALUES
	(6, NULL,'mandat','Mandat', 'globale', 'MND000', 1),
	(7, NULL,'mod','Demandes MOD', 'globale', 'MOD000', 2),
	(8, NULL,'collaborators','Collaborateurs', 'globale', 'ACL000', 3),
	(9, NULL,'history','Historique', 'globale', 'HIS000', 4),
	(1, NULL,'entity','Dossier', 'navigation', 'ACC000', 1),
		-- Insert  entity childs
		(2, 1,'users','Utilisateurs', 'navigation', 'USR000', 2),
		(3, 1,'collaborators','Collaborateurs', 'navigation', 'CLB000', 3),
		(4, 1,'offers','Offres', 'navigation', 'OFF000', 4),
		(5, 1,'informations','Informations', 'navigation', 'INF000', 5),
	(10, NULL,'documents','Documents', 'navigation', 'GED000', 6),
	(11, NULL,'bank','Banque', 'navigation', 'BNK001', 7),
	(12, NULL,'indicators','Indicateurs', 'navigation', 'KPI000', 8)

SET IDENTITY_INSERT [auth].[Resource] ON; 
END