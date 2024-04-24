IF (SELECT count(*) FROM [auth].[Authorization]) = 0
BEGIN
SET IDENTITY_INSERT [auth].[Authorization] ON; 
	INSERT INTO [auth].[Authorization] ([Name], [Description], [Code], [Label], [Category]) VALUES 
		('', '', 'CLTGES001', 'Modifier les administrateurs', 'CltGESTION'),
		('', '', 'CLTGES002', 'Consulter les utilisateurs', 'CltGESTION'),
		('', '', 'CLTGES003', 'Ajouter un utilisateur', 'CltGESTION'),
		('', '', 'CLTGES004', 'Supprimer un utilisateur', 'CltGESTION'),
		('', '', 'CLTGES005', 'Gérer les droits utilisateurs', 'CltGESTION'),
		('', '', 'CLTGES006', 'Accéder aux offres souscrites', 'CltGESTION'),
		('', '', 'CLTGES007', 'Accéder aux informations d''entreprise', 'CltGESTION'),
		('', '', 'CLTGES008', 'Accéder aux factures Pulse', 'CltGESTION'),
		('', '', 'CLTGED001', 'Accéder à la GED Gestion sociale', 'CltGED GS'),
		('', '', 'CLTGED002', 'Accéder à la GED Comptable', 'CltGED ESC'),
		('', '', 'CLTGS001', 'Accéder à l''outil Silae', 'CltGS'),
		('', '', 'CLTGS002', 'Accéder aux EVP', 'CltGS'),
		('', '', 'CLTGS003', 'Accéder à l''embauche salariée', 'CltGS'),
		('', '', 'CLTESC001', 'Accéder à l''outil MEG', 'CltESC'),
		('', '', 'CLTESC002', 'Accéder à l''outil Pennylane', 'CltESC'),
		('', '', 'CLTPIL001', 'Accéder aux rapports BI Financier', 'CltPILOTAGE'),
		('', '', 'CLTPIL002', 'Accéder aux rapports BI RH', 'CltPILOTAGE'),
		('', '', 'CLTPIL003', 'Accéder aux indicateurs', 'CltPILOTAGE'),
		('', '', 'CLTPIL004', 'Accéder aux données bancaires', 'CltPILOTAGE'),
		('', '', '', 'Administration des utilisateurs de la plateforme', 'ColADMIN'),
		('', '', '', 'Onglet Mandat', 'ColESC'),
		('', '', '', 'Déployer une offre', 'ColOFFRE'),
		('', '', '', 'Onglet des demandes MOD', 'ColOFFRE'),
		('', '', '', 'Déposer un rapport BI', 'ColRAPPORT'),
		('', '', '', 'Historique des délégations', 'ColGESTION'),
		('', '', '', 'Historique', 'ColGESTION')
SET IDENTITY_INSERT [auth].[Authorization] OFF;
END