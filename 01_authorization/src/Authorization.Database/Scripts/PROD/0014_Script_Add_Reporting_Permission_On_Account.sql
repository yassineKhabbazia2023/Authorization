-- Création de la table temporaire
IF OBJECT_ID('tempdb..#AccountIdsFromReporting') IS NOT NULL
    DROP TABLE #AccountIdsFromReporting;

CREATE TABLE #AccountIdsFromReporting (AccountId INT);

-- Les accountIds qui possèdent au moins un rapport sur la PROD
INSERT INTO #AccountIdsFromReporting (AccountId)
VALUES (2), (3), (249), (273), (290), (518), (1881), (2281), (2434), (3418), (5038), (5109), (5570), (5584), (6342), (6429), (6930), (6947), (7254),
(7749), (7819), (8372), (8991), (9383), (10090), (11359), (12722), (13009), (13860), (14397), (15957), (16384), (16876), (17256), (17526), (17761),
(18237), (18480), (19049), (19082), (19180), (20519), (20650), (21637), (22037), (22379), (23032), (23219), (23504), (23529), (24231), (24455), (26156),
(26449), (27690), (27966), (28358), (29453), (29758), (30584), (31743), (32044), (32448), (33907), (34500), (35223), (35739), (36061), (36882), (36951),
(37123), (37666), (38490), (38840), (39039), (39529), (39866), (40192), (40373), (41050), (42176), (42514), (43330), (43423), (43446), (43597), (43683),
(43744), (43861), (44162), (44441), (45229), (45954), (46485), (47459), (47851), (48164), (48770), (48851), (49013), (49328), (49348), (50487), (50821),
(51165), (51463), (52022), (52260), (52267), (52666), (53081), (54114), (54165), (54535), (55274), (55382), (56919), (58119), (58123), (58197), (58330),
(58394), (59219), (59306), (59652), (60972), (62051), (62778), (63407), (63550), (63738), (64461), (65082), (65390), (65794), (65873), (66359), (66825),
(66841), (67022), (68006), (68681), (69484), (70025), (71380), (71873), (72170), (72550), (73149), (73404), (74025), (74179), (74180), (74508), (75322),
(75367), (75913), (76004), (77121), (77622), (77889), (77903), (78152), (78759), (79722), (79885), (80015), (80523), (80789), (80849), (81133), (81373),
(81474), (81761), (81797), (81977), (82202), (82772), (84533), (84694), (86133), (88552), (88825), (88920), (89035), (89463), (90661), (90963), (91596),
(91884), (92719), (92979), (93303), (94425), (95400), (96129), (96196), (96384), (96458), (96929), (97599), (97600), (97601), (104309), (105492), (106861),
(107930), (108127), (108402), (108581), (108698), (108822), (109921), (110194), (110401), (110565), (110612), (117666), (117799), (117841), (119483);

BEGIN TRY
    BEGIN TRANSACTION;
		-- Tous les entités qui ont une permissions CORAPP001
		WITH AccountWithCORAPP001 AS (
			SELECT DISTINCT AccountId
			FROM [auth].[AccountAuthorization] aa
			JOIN [auth].[Authorization] a ON aa.AuthorizationId = a.AuthorizationId
			WHERE a.Code = 'CORAPP001'
		)

		-- Insertion des permissions CORAPP001 manquantes
		INSERT INTO [auth].[AccountAuthorization] (AccountId, AuthorizationId, Enabled)
		SELECT t.AccountId, a.AuthorizationId, 1
		FROM #AccountIdsFromReporting t
		LEFT JOIN AccountWithCORAPP001 c ON t.AccountId = c.AccountId
		CROSS JOIN [auth].[Authorization] a
		WHERE c.AccountId IS NULL AND a.Code = 'CORAPP001';

		-- Tous les entités qui ont une permissions CLRAPP001
		WITH AccountWithCLRAPP001 AS (
			SELECT DISTINCT AccountId
			FROM [auth].[AccountAuthorization] aa
			JOIN [auth].[Authorization] a ON aa.AuthorizationId = a.AuthorizationId
			WHERE a.Code = 'CLRAPP001'
		)

		-- Insertion des permissions CLRAPP001 manquantes
		INSERT INTO [auth].[AccountAuthorization] (AccountId, AuthorizationId, Enabled)
		SELECT t.AccountId, a.AuthorizationId, 1
		FROM #AccountIdsFromReporting t
		LEFT JOIN AccountWithCLRAPP001 c ON t.AccountId = c.AccountId
		CROSS JOIN [auth].[Authorization] a
		WHERE c.AccountId IS NULL AND a.Code = 'CLRAPP001';

		-- Tous les contacts signataires qui ont une permissions CLRAPP001
		WITH ContactSignatoryWithCLRAPP001 AS (
			SELECT DISTINCT c.ContactId, c.AccountId
			FROM [auth].[ContactAuthorization] c
			JOIN [account].[Role] r ON r.AccountId = c.AccountId AND r.ContactId = c.ContactId
			JOIN [auth].[Authorization] a ON c.AuthorizationId = a.AuthorizationId
			WHERE r.IsSignatory = 1 AND a.Code = 'CLRAPP001'
		),
		ContactSignatoryFromReporting AS (
			SELECT r.ContactId, r.AccountId
			FROM [account].[Role] r
			JOIN #AccountIdsFromReporting temp ON r.AccountId = temp.AccountId
			WHERE IsSignatory = 1
		)

		-- Insertion des permissions contacts signataires manquantes
		INSERT INTO [auth].[ContactAuthorization] (ContactId, AccountId, AuthorizationId, CreationDate)
		SELECT r.ContactId, r.AccountId, a.AuthorizationId, GETUTCDATE()
		FROM ContactSignatoryFromReporting r
		LEFT JOIN ContactSignatoryWithCLRAPP001 c ON r.AccountId = c.AccountId AND r.ContactId = c.ContactId
		CROSS JOIN [auth].[Authorization] a
		WHERE c.AccountId IS NULL AND a.Code = 'CLRAPP001';
	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW;
END CATCH;

-- Drop table temporaire
DROP TABLE #AccountIdsFromReporting;