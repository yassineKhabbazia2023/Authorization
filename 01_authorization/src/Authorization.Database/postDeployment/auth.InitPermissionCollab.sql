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
where AuthorizationId >= 20 and AuthorizationId <= 28

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

CREATE TABLE #role(
		AccountId int,
		ContactId int)
	INSERT INTO #role(AccountId, ContactId)
	VALUES
	(1,1),
	(2,1),
	(3,1),
	(4,2),
	(5,2),
	(6,2),
	(7,3),
	(8,3),
	(9,3),
	(10,4),
	(11,4),
	(12,4),
	(13,5),
	(14,5),
	(15,5),
	(16,6),
	(17,6),
	(18,6),
	(19,7),
	(20,7),
	(21,7),
	(22,8),
	(23,8),
	(24,8),
	(25,9),
	(26,9),
	(27,9),
	(28,10),
	(29,10),
	(30,10),
	(31,11),
	(32,11),
	(33,11),
	(34,12),
	(35,12),
	(36,12),
	(37,13),
	(38,13),
	(39,13),
	(40,14),
	(41,14),
	(42,14),
	(43,15),
	(44,15),
	(45,15),
	(46,16),
	(47,16),
	(48,16),
	(49,17),
	(50,17),
	(51,17),
	(52,18),
	(53,18),
	(54,18),
	(76,19),
	(77,19),
	(78,19),
	(55,20),
	(56,20),
	(57,20),
	(58,21),
	(59,21),
	(60,21),
	(61,22),
	(62,22),
	(63,22),
	(64,23),
	(65,23),
	(66,23),
	(67,24),
	(68,24),
	(69,24),
	(70,25),
	(71,25),
	(72,25),
	(73,26),
	(74,26),
	(75,26),
	(76,27),
	(77,27),
	(78,27),
	(79,28),
	(80,28),
	(81,28),
	(82,29),
	(83,29),
	(84,29),
	(85,30),
	(86,30),
	(87,30),
	(88,31),
	(89,31),
	(90,31),
	(91,32),
	(92,32),
	(93,32),
	(94,33),
	(95,33),
	(96,33),
	(97,34),
	(98,34),
	(99,34),
	(100,35),
	(101,35),
	(102,35),
	(103,36),
	(104,36),
	(105,36),
	(106,37),
	(107,37),
	(108,37),
	(109,38),
	(110,38),
	(111,38),
	(112,39),
	(113,39),
	(114,39),
	(115,40),
	(116,40),
	(117,40),
	(118,41),
	(119,41),
	(120,41),
	(121,42),
	(122,42),
	(123,42),
	(124,43),
	(125,43),
	(126,43),
	(127,44),
	(128,44),
	(129,44),
	(130,45),
	(131,45),
	(132,45),
	(133,46),
	(134,46),
	(135,46),
	(136,47),
	(137,47),
	(138,47),
	(139,48),
	(140,48),
	(141,48),
	(142,49),
	(143,49),
	(144,49),
	(145,50),
	(146,50),
	(147,50),
	(148,51),
	(149,51),
	(150,51),
	(151,52),
	(152,52),
	(153,52),
	(154,53),
	(155,53),
	(156,53),
	(157,54),
	(158,54),
	(159,54),
	(160,55),
	(161,55),
	(162,55),
	(424,161),
	(425,161),
	(426,161),
	(427,162),
	(428,162),
	(429,162)
-- Assign authorization for all Collab (cas ibs -1)
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
	where AuthorizationId >= 20 and AuthorizationId <= 28

	-- Assign authorization for all Collab (cas ibs 601, 602, 603)
	INSERT INTO auth.ContactAuthorization(ContactId, AccountId, AuthorizationId, CreationDate)
	SELECT
		@contactId,
		601,
		AuthorizationId,
		@date
	FROM auth.[Authorization]
	where AuthorizationId > 28
	
	INSERT INTO auth.ContactAuthorization(ContactId, AccountId, AuthorizationId, CreationDate)
	SELECT
		@contactId,
		602,
		AuthorizationId,
		@date
	FROM auth.[Authorization]
	where AuthorizationId > 28
	
	INSERT INTO auth.ContactAuthorization(ContactId, AccountId, AuthorizationId, CreationDate)
	SELECT
		@contactId,
		603,
		AuthorizationId,
		@date
	FROM auth.[Authorization]
	where AuthorizationId > 28

	-- Assign authorization for all Collab (cas ibs particulier pour chaque collab)
	DECLARE @offset int = 0
	WHILE @offset <= 2
	BEGIN
		INSERT INTO auth.ContactAuthorization(ContactId, AccountId, AuthorizationId, CreationDate)
		SELECT
			@contactId,
			(select AccountId from #role Where ContactId = @contactId ORDER BY AccountId OFFSET @offset ROWS FETCH NEXT 1 ROWS ONLY) as AccountId,
			AuthorizationId,
			@date
		FROM auth.[Authorization] 
		where AuthorizationId > 28
		SELECT @offset = @offset + 1
	END

	FETCH NEXT FROM contactId_cursor
	INTO @contactId
END

CLOSE contactId_cursor
DEALLOCATE contactId_cursor
DROP TABLE #role