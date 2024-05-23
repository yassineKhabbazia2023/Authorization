-- Clean authorization
DELETE FROM auth.[Authorization]
-- Initialize table Authorization
DBCC CHECKIDENT ('[auth].[Authorization]', RESEED, 0)

INSERT INTO auth.[Authorization] (
	Name, Description, Code, Label, Category, Configurable, [View], [ProductCode])
	VALUES
	('Super Admin', '', 'CLADMI001', 'Droit Super Admin client', 'CLTGESTION', '0', 'Global', NULL),
	('View User', '', 'CLUSER001', 'Consulter les utilisateurs', 'CLTGESTION', '1', 'Both', NULL),
	('Add User', '', 'CLUSER002', 'Ajouter un utilisateur', 'CLTGESTION', '1', 'Partial',NULL),
	('Delete User', '', 'CLUSER003', 'Supprimer un utilisateur', 'CLTGESTION', '1', 'Partial',NULL),
	('Update right', '', 'CLUSER004', 'Gérer les droits utilisateurs', 'CLTGESTION', '1', 'Partial', NULL),
	('View offers', '', 'CLOFF001', 'Accéder aux offres souscrites', 'CLTGESTION', '1', 'Partial', NULL),
	('View informations', '', 'CLINFO001', 'Accéder aux informations d''entreprise', 'CLTGESTION', '1', 'Partial', NULL),
	('View Invoice', '', 'CLINVO001', 'Accéder aux factures Pulse', 'CLTGESTION', '0', 'Partial', NULL),
	('View GED GS', '', 'CLGED0001', 'Accéder à la GED Gestion sociale', 'CLTGED GS', '1', 'Partial', 'gedgs'),
	('View GED ESC', '', 'CLGED0002', 'Accéder à la GED Comptable', 'CLTGED ESC', '1', 'Partial', 'gedesc'),
	('Access Silae', '', 'CLSILA001', 'Accéder à l''outil Silae', 'CLTGS', '1', 'Partial', 'silaeaccess'),
	('View EVP', '', 'CLEVP001', 'Accéder aux EVP', 'CLTGS', '0', 'Partial', 'EVP'),
	('View hiring', '', 'CLEMB001', 'Accéder à l''embauche salariée', 'CLTGS', '0', 'Partial', 'Hiring'),
	('Access MEG', '', 'CLMEG001', 'Accéder à l''outil MEG', 'CLTESC', '1', 'Partial', 'megaccess'),
	('Access Pennylane', '', 'CLPEN001', 'Accéder à l''outil Pennylane', 'CLTESC', '1', 'Partial','pennylaneaccess'),
	('View bi financial', '', 'CLRAPP001', 'Accéder aux rapports BI Financier', 'CLTPILOTAGE', '1', 'Partial',NULL),
	('View bi HR', '', 'CLRAPP002', 'Accéder aux rapports BI RH', 'CLTPILOTAGE', '0', 'Partial', NULL),
	('View kpi', '', 'CLKPI0001', 'Accéder aux indicateurs', 'CLTPILOTAGE', '0', 'Partial', NULL),
	('View bank', '', 'CLBANK001', 'Accéder aux données bancaires', 'CLTPILOTAGE', '0', 'Partial', NULL),
	('View collabs', '', 'COADMI001', 'Accéder à l''onglet collaborateurs', 'COLADMIN', '1', 'Global', NULL),
	('Delete collab', '', 'COADMI002', 'Supprimer un collaborateur', 'COLADMIN', '1', 'Global', NULL),
	('Update right collab', '', 'COADMI003', 'Gérer les droits des collaborateurs', 'COLADMIN', '1', 'Global', NULL),
	('Deploy subscription', '', 'COOFF001', 'Activer une offre', 'COLOFFRE', '1', 'Global', NULL),
	('Activate subsciption', '', 'COOFF002', 'Onglet des demandes MOD', 'COLOFFRE', '1', 'Global', NULL),
	('Add report', '', 'CORAP001', 'Déposer un rapport BI', 'COLRAPPORT', '1', 'Global', NULL),
	('View delegation', '', 'COGES001', 'Historique des délégations', 'COLGESTION', '1', 'Both', NULL),
	('View Calendar', '', 'COCAL001', 'Accéder au calendrier', 'COLGESTION', '1', 'Header', NULL),
	('View mandate', '', 'COMAND001', 'Onglet Mandat', 'COLPROD', '1', 'Global', NULL),
	('Mirror GED GS', '', 'COGED0001', 'Accéder à la GED Gestion sociale', 'COLPROD', '1', 'Partial', NULL),
	('Mirror GED ESC', '', 'COGED0002', 'Accéder à la GED Comptable', 'COLPROD', '1', 'Partial', NULL),
	('Mirror users customer', '', 'COUSER001', 'Accéder à l''onglet utilisateurs', 'COLMIRROIR', '1', 'Partial', NULL),
	('Mirror users collab', '', 'COUSER002', 'Accéder à l''onglet collaborateurs', 'COLMIRROIR', '1', 'Partial', NULL),
	('Mirror offer', '', 'COOFF003', 'Accéder à l''onglet offre', 'COLMIRROIR', '1', 'Partial', NULL),
	('Mirror informations', '', 'COINFO001', 'Accéder à l''onglet informations', 'COLMIRROIR', '1', 'Partial', NULL),
	('Mirror invoice', '', 'COINVO001', 'Accéder aux factures Pulse', 'COLMIRROIR', '0', 'Partial', NULL),
	('Mirror EVP', '', 'COEVPO01', 'Accéder aux EVP', 'COLMIRROIR', '0', 'Partial'),
	('Mirror hiring', '', 'COEMB001', 'Accéder à l''embauche salariée', 'COLMIRROIR', '0', 'Partial', NULL),
	('Mirror launcher', '', 'COLANC001', 'Accéder aux lançeurs d''outils externes', 'COLMIRROIR', '1', 'Partial', NULL),
	('Mirror report', '', 'CORAPP001', 'Accéder aux rapports BI ', 'COLMIRROIR', '1', 'Partial', NULL),
	('Mirror kpi', '', 'COKPI0001', 'Accéder aux indicateurs', 'COLMIRROIR', '0', 'Partial', NULL),
	('Mirror bank', '', 'COBANK001', 'Accéder aux données bancaires', 'COLMIRROIR', '0', 'Partial', NULL)
