IF NOT EXISTS (SELECT * FROM [auth].[Resource] WHERE Name = 'mandat')
BEGIN
SET IDENTITY_INSERT [auth].[Resource] ON
INSERT INTO [auth].[Resource](ResourceId,ParentId, Name, Label, Category, IsVisible, Code, OrderShow, Type) VALUES
	(6, NULL,'mandat','Mandat', 'globale', 1, 'MND000', 1, 'Collaborator'),
	(7, NULL,'mod','Demandes MOD', 'globale', 1, 'MOD000', 2, 'Collaborator'),
	(8, NULL,'collaborators','Collaborateurs', 'globale', 1, 'ACL000', 3, 'Collaborator'),
	(9, NULL,'history','Historique', 'globale', 1, 'HIS000', 4, 'Collaborator'),
	(1, NULL,'entity','Dossier', 'navigation', 1, 'ACC000', 1, 'Customer'),
		-- Insert  entity childs
		(2, 1,'users','Utilisateurs', 'navigation', 1, 'USR000', 2, 'Customer'),
		(3, 1,'collaborators','Collaborateurs', 'navigation', 1, 'CLB000', 3, 'Customer'),
		(4, 1,'offers','Offres', 'navigation', 1, 'OFF000', 4, 'Customer'),
		(5, 1,'informations','Informations', 'navigation', 1, 'INF000', 5, 'Customer'),
	(10, NULL,'documents','Documents', 'navigation', 1, 'GED000', 6, 'Customer'),
	(11, NULL,'bank','Banque', 'navigation', 1, 'BNK001', 7, 'Customer'),
	(12, NULL,'indicators','Indicateurs', 'navigation', 1, 'KPI000', 8, 'Customer')
END