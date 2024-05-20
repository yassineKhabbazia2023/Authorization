-- Clean authorization
DELETE FROM auth.AccountAuthorization
DELETE FROM auth.ContactAuthorization
DELETE FROM auth.[Authorization]

-- Initialize table Authorization
DBCC CHECKIDENT ('[auth].[Authorization]', RESEED, 0)

INSERT INTO auth.[Authorization] (
	Name, Description, Code, Label, Category, Configurable)
VALUES
('Super Admin', '', 'CLADMI001', 'Droit Super Admin client', 'CLTGESTION', '0'),
('View User', '', 'CLUSER001', 'Consulter les utilisateurs', 'CLTGESTION', '1'),
('Add User', '', 'CLUSER002', 'Ajouter un utilisateur', 'CLTGESTION', '1'),
('Delete User', '', 'CLUSER003', 'Supprimer un utilisateur', 'CLTGESTION', '1'),
('Update right', '', 'CLUSER004', 'Gérer les droits utilisateurs', 'CLTGESTION', '1'),
('View offers', '', 'CLOFF001', 'Accéder aux offres souscrites', 'CLTGESTION', '1'),
('View informations', '', 'CLINFO001', 'Accéder aux informations d''entreprise', 'CLTGESTION', '1'),
('View Invoice', '', 'CLINVO001', 'Accéder aux factures Pulse', 'CLTGESTION', '0'),
('View GED GS', '', 'CLGED0001', 'Accéder à la GED Gestion sociale', 'CLTGED GS', '1'),
('View GED ESC', '', 'CLGED0002', 'Accéder à la GED Comptable', 'CLTGED ESC', '1'),
('Access Silae', '', 'CLSILA001', 'Accéder à l''outil Silae', 'CLTGS', '1'),
('View EVP', '', 'CLEVP001', 'Accéder aux EVP', 'CLTGS', '0'),
('View hiring', '', 'CLEMB001', 'Accéder à l''embauche salariée', 'CLTGS', '0'),
('Access MEG', '', 'CLMEG001', 'Accéder à l''outil MEG', 'CLTESC', '1'),
('Access Pennylane', '', 'CLPEN001', 'Accéder à l''outil Pennylane', 'CLTESC', '1'),
('View bi financial', '', 'CLRAPP001', 'Accéder aux rapports BI Financier', 'CLTPILOTAGE', '1'),
('View bi HR', '', 'CLRAPP002', 'Accéder aux rapports BI RH', 'CLTPILOTAGE', '0'),
('View kpi', '', 'CLKPI0001', 'Accéder aux indicateurs', 'CLTPILOTAGE', '0'),
('View bank', '', 'CLBANK001', 'Accéder aux données bancaires', 'CLTPILOTAGE', '0'),
('View collabs', '', 'COADMI001', 'Accéder à l''onglet collaborateurs', 'COLADMIN', '1'),
('Delete collab', '', 'COADMI002', 'Supprimer un collaborateur', 'COLADMIN', '1'),
('Update right collab', '', 'COADMI003', 'Gérer les droits des collaborateurs', 'COLADMIN', '1'),
('Deploy subscription', '', 'COOFF001', 'Activer une offre', 'COLOFFRE', '1'),
('Activate subsciption', '', 'COOFF002', 'Onglet des demandes MOD', 'COLOFFRE', '1'),
('Add report', '', 'CORAP001', 'Déposer un rapport BI', 'COLRAPPORT', '1'),
('View delegation', '', 'COGES001', 'Historique des délégations', 'COLGESTION', '1'),
('View Calendar', '', 'COCAL001', 'Accéder au calendrier', 'COLGESTION', '1'),
('View mandate', '', 'COMAND001', 'Onglet Mandat', 'COLPROD', '1'),
('Mirror GED GS', '', 'COGED0001', 'Accéder à la GED Gestion sociale', 'COLPROD', '1'),
('Mirror GED ESC', '', 'COGED0002', 'Accéder à la GED Comptable', 'COLPROD', '1'),
('Mirror users customer', '', 'COUSER001', 'Accéder à l''onglet utilisateurs', 'COLMIRROIR', '1'),
('Mirror users collab', '', 'COUSER002', 'Accéder à l''onglet collaborateurs', 'COLMIRROIR', '1'),
('Mirror offer', '', 'COOFF003', 'Accéder à l''onglet offre', 'COLMIRROIR', '1'),
('Mirror informations', '', 'COINFO001', 'Accéder à l''onglet informations', 'COLMIRROIR', '1'),
('Mirror invoice', '', 'COINVO001', 'Accéder aux factures Pulse', 'COLMIRROIR', '0'),
('Mirror EVP', '', 'COEVPO01', 'Accéder aux EVP', 'COLMIRROIR', '0'),
('Mirror hiring', '', 'COEMB001', 'Accéder à l''embauche salariée', 'COLMIRROIR', '0'),
('Mirror launcher', '', 'COLANC001', 'Accéder aux lançeurs d''outils externes', 'COLMIRROIR', '1'),
('Mirror report', '', 'CORAPP001', 'Accéder aux rapports BI ', 'COLMIRROIR', '1'),
('Mirror kpi', '', 'COKPI0001', 'Accéder aux indicateurs', 'COLMIRROIR', '0'),
('Mirror bank', '', 'COBANK001', 'Accéder aux données bancaires', 'COLMIRROIR', '0')

-- Assign authorization for Account -1
INSERT INTO auth.AccountAuthorization(AccountId, AuthorizationId, Enabled)
SELECT
	-1,
	AuthorizationId,
	1
FROM auth.[Authorization]
where AuthorizationId >= 20

-- Assign all authorization for all account
DECLARE @AccId int = 1
DECLARE @MaxAccId int
SELECT @MaxAccId = MAX(AccountId) from account.Account

WHILE @AccId <= @MaxAccId
BEGIN
	INSERT INTO auth.AccountAuthorization(AccountId, AuthorizationId, Enabled)
	SELECT
		@AccId,
		AuthorizationId,
		1
	FROM auth.[Authorization]
	SELECT @AccId = @AccId + 1
END


-- Assign authorization for all Collab
DECLARE @date DATETIME = GETDATE()
DECLARE @contactId int
DECLARE contactId_cursor CURSOR FOR
SELECT ContactId FROM actor.Contact Where Type = 'Collaborator'

OPEN contactId_cursor

FETCH NEXT FROM contactId_cursor
INTO @contactId

WHILE @@FETCH_STATUS = 0
BEGIN
	INSERT INTO auth.ContactAuthorization(ContactId, AccountId, AuthorizationId, CreationDate)
	SELECT
		@contactId,
		-1,
		AuthorizationId,
		@date
	FROM auth.[Authorization]
	where AuthorizationId >= 20

	FETCH NEXT FROM contactId_cursor
	INTO @contactId
END

CLOSE contactId_cursor
DEALLOCATE contactId_cursor
DROP TABLE #role