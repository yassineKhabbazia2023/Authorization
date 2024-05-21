-- Clean authorization
DELETE FROM auth.AccountAuthorization
DELETE FROM auth.ContactAuthorization
DELETE FROM auth.[Authorization]

-- Initialize table Authorization
DBCC CHECKIDENT ('[auth].[Authorization]', RESEED, 0)

INSERT INTO auth.[Authorization] (
	Name, Description, Code, Label, Category, Configurable, [View], [ProductCode])
	VALUES
	('Super Admin', '', 'CLADMI001', 'Droit Super Admin client', 'CLTGESTION', '0', 'Global'),
	('View User', '', 'CLUSER001', 'Consulter les utilisateurs', 'CLTGESTION', '1', 'Both'),
	('Add User', '', 'CLUSER002', 'Ajouter un utilisateur', 'CLTGESTION', '1', 'Partial'),
	('Delete User', '', 'CLUSER003', 'Supprimer un utilisateur', 'CLTGESTION', '1', 'Partial'),
	('Update right', '', 'CLUSER004', 'Gérer les droits utilisateurs', 'CLTGESTION', '1', 'Partial'),
	('View offers', '', 'CLOFF001', 'Accéder aux offres souscrites', 'CLTGESTION', '1', 'Partial'),
	('View informations', '', 'CLINFO001', 'Accéder aux informations d''entreprise', 'CLTGESTION', '1', 'Partial'),
	('View Invoice', '', 'CLINVO001', 'Accéder aux factures Pulse', 'CLTGESTION', '0', 'Partial'),
	('View GED GS', '', 'CLGED0001', 'Accéder à la GED Gestion sociale', 'CLTGED GS', '1', 'Partial', 'gedgs'),
	('View GED ESC', '', 'CLGED0002', 'Accéder à la GED Comptable', 'CLTGED ESC', '1', 'Partial', 'gedesc'),
	('Access Silae', '', 'CLSILA001', 'Accéder à l''outil Silae', 'CLTGS', '1', 'Partial', 'silaeaccess'),
	('View EVP', '', 'CLEVP001', 'Accéder aux EVP', 'CLTGS', '0', 'Partial', 'EVP'),
	('View hiring', '', 'CLEMB001', 'Accéder à l''embauche salariée', 'CLTGS', '0', 'Partial', 'Hiring'),
	('Access MEG', '', 'CLMEG001', 'Accéder à l''outil MEG', 'CLTESC', '1', 'Partial', 'megaccess'),
	('Access Pennylane', '', 'CLPEN001', 'Accéder à l''outil Pennylane', 'CLTESC', '1', 'Partial'),
	('View bi financial', '', 'CLRAPP001', 'Accéder aux rapports BI Financier', 'CLTPILOTAGE', '1', 'Partial'),
	('View bi HR', '', 'CLRAPP002', 'Accéder aux rapports BI RH', 'CLTPILOTAGE', '0', 'Partial'),
	('View kpi', '', 'CLKPI0001', 'Accéder aux indicateurs', 'CLTPILOTAGE', '0', 'Partial'),
	('View bank', '', 'CLBANK001', 'Accéder aux données bancaires', 'CLTPILOTAGE', '0', 'Partial'),
	('View collabs', '', 'COADMI001', 'Accéder à l''onglet collaborateurs', 'COLADMIN', '1', 'Global'),
	('Delete collab', '', 'COADMI002', 'Supprimer un collaborateur', 'COLADMIN', '1', 'Global'),
	('Update right collab', '', 'COADMI003', 'Gérer les droits des collaborateurs', 'COLADMIN', '1', 'Global'),
	('Deploy subscription', '', 'COOFF001', 'Activer une offre', 'COLOFFRE', '1', 'Global'),
	('Activate subsciption', '', 'COOFF002', 'Onglet des demandes MOD', 'COLOFFRE', '1', 'Global'),
	('Add report', '', 'CORAP001', 'Déposer un rapport BI', 'COLRAPPORT', '1', 'Global'),
	('View delegation', '', 'COGES001', 'Historique des délégations', 'COLGESTION', '1', 'Both'),
	('View Calendar', '', 'COCAL001', 'Accéder au calendrier', 'COLGESTION', '1', 'Header'),
	('View mandate', '', 'COMAND001', 'Onglet Mandat', 'COLPROD', '1', 'Global'),
	('Mirror GED GS', '', 'COGED0001', 'Accéder à la GED Gestion sociale', 'COLPROD', '1', 'Partial'),
	('Mirror GED ESC', '', 'COGED0002', 'Accéder à la GED Comptable', 'COLPROD', '1', 'Partial'),
	('Mirror users customer', '', 'COUSER001', 'Accéder à l''onglet utilisateurs', 'COLMIRROIR', '1', 'Partial'),
	('Mirror users collab', '', 'COUSER002', 'Accéder à l''onglet collaborateurs', 'COLMIRROIR', '1', 'Partial'),
	('Mirror offer', '', 'COOFF003', 'Accéder à l''onglet offre', 'COLMIRROIR', '1', 'Partial'),
	('Mirror informations', '', 'COINFO001', 'Accéder à l''onglet informations', 'COLMIRROIR', '1', 'Partial'),
	('Mirror invoice', '', 'COINVO001', 'Accéder aux factures Pulse', 'COLMIRROIR', '0', 'Partial'),
	('Mirror EVP', '', 'COEVPO01', 'Accéder aux EVP', 'COLMIRROIR', '0', 'Partial'),
	('Mirror hiring', '', 'COEMB001', 'Accéder à l''embauche salariée', 'COLMIRROIR', '0', 'Partial'),
	('Mirror launcher', '', 'COLANC001', 'Accéder aux lançeurs d''outils externes', 'COLMIRROIR', '1', 'Partial'),
	('Mirror report', '', 'CORAPP001', 'Accéder aux rapports BI ', 'COLMIRROIR', '1', 'Partial'),
	('Mirror kpi', '', 'COKPI0001', 'Accéder aux indicateurs', 'COLMIRROIR', '0', 'Partial'),
	('Mirror bank', '', 'COBANK001', 'Accéder aux données bancaires', 'COLMIRROIR', '0', 'Partial')

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