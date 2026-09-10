-- =============================================
-- Script: Grant CLRAPNM001 to Pennylane accounts and their signatories
-- Date: 2026-09-07
-- Target database: authorization
-- Description: RATTRAPAGE du parc existant. Attribue la permission CLRAPNM001
--              (note mensuelle comptable) à l'ensemble des dossiers ayant l'offre
--              Pennylane activée, ainsi qu'à leurs signataires.
--              Adapté de 0014_Script_Add_Reporting_Permission_On_Account.sql.
--              Anti-doublon + idempotent : le script peut être rejoué sans effet.
--
--              À exécuter au Go PM. Non piloté par le feature flag.
--              Prérequis : 0033_Add_CLRAPNM001_Permission.sql (la permission doit
--              exister avant d'être attribuée).
--
--              Critère « offre Pennylane activée » aligné sur la définition
--              utilisée par l'attribution automatique
--              (AuthorizationEventRepository.IsPennylaneActivatedAsync) :
--              compte actif possédant au moins une autorisation Pennylane
--              (CLPEN001, COPEN001, CLGEDPEN01).
-- =============================================

BEGIN TRY
	BEGIN TRANSACTION;

		-- Résolution de la permission CLRAPNM001
		DECLARE @AuthorizationId INT;
		SELECT @AuthorizationId = AuthorizationId
		FROM [auth].[Authorization]
		WHERE Code = 'CLRAPNM001';

		IF @AuthorizationId IS NULL
		BEGIN
			PRINT 'ERROR: Permission CLRAPNM001 not found. Run 0033_Add_CLRAPNM001_Permission.sql first.';
			ROLLBACK TRANSACTION;
			RETURN;
		END

		-- Dossiers ayant l'offre Pennylane activée
		IF OBJECT_ID('tempdb..#PennylaneAccounts') IS NOT NULL
			DROP TABLE #PennylaneAccounts;

		CREATE TABLE #PennylaneAccounts (AccountId INT PRIMARY KEY);

		INSERT INTO #PennylaneAccounts (AccountId)
		SELECT DISTINCT a.AccountId
		FROM [account].[Account] a
		INNER JOIN [auth].[AccountAuthorization] aa ON aa.AccountId = a.AccountId
		INNER JOIN [auth].[Authorization] auth ON auth.AuthorizationId = aa.AuthorizationId
		WHERE a.IsActive = 1
			AND auth.Code IN ('CLPEN001', 'COPEN001', 'CLGEDPEN01');

		-- 1) Attribution CLRAPNM001 aux dossiers Pennylane (anti-doublon)
		INSERT INTO [auth].[AccountAuthorization] (AccountId, AuthorizationId, Enabled)
		SELECT p.AccountId, @AuthorizationId, 1
		FROM #PennylaneAccounts p
		WHERE NOT EXISTS (
			SELECT 1
			FROM [auth].[AccountAuthorization] aa
			WHERE aa.AccountId = p.AccountId
				AND aa.AuthorizationId = @AuthorizationId
		);

		DECLARE @AccountsAdded INT = @@ROWCOUNT;
		PRINT 'CLRAPNM001 permission added to ' + CAST(@AccountsAdded AS VARCHAR(10)) + ' accounts';

		-- 2) Attribution CLRAPNM001 aux signataires de ces dossiers (anti-doublon)
		INSERT INTO [auth].[ContactAuthorization] (ContactId, AccountId, AuthorizationId, CreationDate)
		SELECT r.ContactId, r.AccountId, @AuthorizationId, GETUTCDATE()
		FROM [account].[Role] r
		INNER JOIN #PennylaneAccounts p ON p.AccountId = r.AccountId
		WHERE r.IsSignatory = 1
			AND NOT EXISTS (
				SELECT 1
				FROM [auth].[ContactAuthorization] ca
				WHERE ca.ContactId = r.ContactId
					AND ca.AccountId = r.AccountId
					AND ca.AuthorizationId = @AuthorizationId
			);

		DECLARE @SignatoriesAdded INT = @@ROWCOUNT;
		PRINT 'CLRAPNM001 permission added to ' + CAST(@SignatoriesAdded AS VARCHAR(10)) + ' signatories';

		DROP TABLE #PennylaneAccounts;

	COMMIT TRANSACTION;
	PRINT 'Script 0035 completed successfully';
END TRY
BEGIN CATCH
	IF @@TRANCOUNT > 0
		ROLLBACK TRANSACTION;
	IF OBJECT_ID('tempdb..#PennylaneAccounts') IS NOT NULL
		DROP TABLE #PennylaneAccounts;
	THROW;
END CATCH;
GO
