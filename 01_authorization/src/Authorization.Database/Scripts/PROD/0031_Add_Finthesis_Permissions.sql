-- =============================================
-- Script: Add Finthesis permissions
-- Date: 2026-08-24
-- Description: Add the 3 Finthesis permissions (2 collaborator, 1 customer).
--              ProductCode = 'Finthesis' triggers automatic assignment on offer activation.
-- =============================================

-- COFIN001: Deposer un rapport Finthesis
IF (SELECT COUNT(*) FROM [auth].[Authorization] WHERE Code = 'COFIN001') = 0
BEGIN
    INSERT INTO [auth].[Authorization] (
        [Name], [Description], [Code], [Label], [Category], [Configurable], [View], [ProductCode], [Type], [TargetAccountType])
    VALUES
        ('Add Finthesis report', '', 'COFIN001', 'Déposer un rapport Finthesis', 'COLRAPPORT', 1, 'Partial', 'Finthesis', 'collaborator', 'Client')

    PRINT 'Permission COFIN001 added';
END
ELSE
BEGIN
    PRINT 'Permission COFIN001 already exists';
END
GO

-- COFINM001: Acceder aux rapports Finthesis (vue miroir)
IF (SELECT COUNT(*) FROM [auth].[Authorization] WHERE Code = 'COFINM001') = 0
BEGIN
    INSERT INTO [auth].[Authorization] (
        [Name], [Description], [Code], [Label], [Category], [Configurable], [View], [ProductCode], [Type], [TargetAccountType])
    VALUES
        ('Mirror Finthesis reports', '', 'COFINM001', 'Accéder aux rapports Finthesis', 'COLMIRROIR', 1, 'Partial', 'Finthesis', 'collaborator', 'Client')

    PRINT 'Permission COFINM001 added';
END
ELSE
BEGIN
    PRINT 'Permission COFINM001 already exists';
END
GO

-- CLFIN001: Acceder aux rapports Finthesis (client)
IF (SELECT COUNT(*) FROM [auth].[Authorization] WHERE Code = 'CLFIN001') = 0
BEGIN
    INSERT INTO [auth].[Authorization] (
        [Name], [Description], [Code], [Label], [Category], [Configurable], [View], [ProductCode], [Type], [TargetAccountType])
    VALUES
        ('View Finthesis reports', '', 'CLFIN001', 'Accéder aux rapports Finthesis', 'CLTPILOTAGE', 1, 'Partial', 'Finthesis', 'customer', 'Client')

    PRINT 'Permission CLFIN001 added';
END
ELSE
BEGIN
    PRINT 'Permission CLFIN001 already exists';
END
GO
