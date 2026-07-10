-- =============================================
-- Script: Add Prospect permissions
-- Date: 2026-07-07
-- Description: Add prospect-specific permissions for configuration sections and documents
-- =============================================

-- CLPCONF001: Accéder à la section Bénéficiaires Effectifs
IF (SELECT COUNT(*) FROM [auth].[Authorization] WHERE Code = 'CLPCONF001') = 0
BEGIN
    INSERT INTO [auth].[Authorization] (
        [Name], [Description], [Code], [Label], [Category], [Configurable], [View], [ProductCode], [Type], [TargetAccountType])
    VALUES
        ('Access Beneficial Owners Section', '', 'CLPCONF001', 'Accéder à la section Bénéficiaires Effectifs', 'CLTPCONF', 1, 'Partial', NULL, 'customer', 'Prospect')

    PRINT 'Permission CLPCONF001 added';
END
ELSE
BEGIN
    PRINT 'Permission CLPCONF001 already exists';
END
GO

-- CLPCONF002: Accéder à la section Documents
IF (SELECT COUNT(*) FROM [auth].[Authorization] WHERE Code = 'CLPCONF002') = 0
BEGIN
    INSERT INTO [auth].[Authorization] (
        [Name], [Description], [Code], [Label], [Category], [Configurable], [View], [ProductCode], [Type], [TargetAccountType])
    VALUES
        ('Access Documents Section', '', 'CLPCONF002', 'Accéder à la section Documents', 'CLTPCONF', 1, 'Partial', NULL, 'customer', 'Prospect')

    PRINT 'Permission CLPCONF002 added';
END
ELSE
BEGIN
    PRINT 'Permission CLPCONF002 already exists';
END
GO

-- CLPCONF004: Accéder à la section Proposition commerciale
IF (SELECT COUNT(*) FROM [auth].[Authorization] WHERE Code = 'CLPCONF004') = 0
BEGIN
    INSERT INTO [auth].[Authorization] (
        [Name], [Description], [Code], [Label], [Category], [Configurable], [View], [ProductCode], [Type], [TargetAccountType])
    VALUES
        ('Access Commercial Proposal Section', '', 'CLPCONF004', 'Accéder à la section Proposition commerciale', 'CLTPCONF', 1, 'Partial', NULL, 'customer', 'Prospect')

    PRINT 'Permission CLPCONF004 added';
END
ELSE
BEGIN
    PRINT 'Permission CLPCONF004 already exists';
END
GO

-- CLPCONF005: Accéder à la section Moyen de paiement
IF (SELECT COUNT(*) FROM [auth].[Authorization] WHERE Code = 'CLPCONF005') = 0
BEGIN
    INSERT INTO [auth].[Authorization] (
        [Name], [Description], [Code], [Label], [Category], [Configurable], [View], [ProductCode], [Type], [TargetAccountType])
    VALUES
        ('Access Payment Method Section', '', 'CLPCONF005', 'Accéder à la section Moyen de paiement', 'CLTPCONF', 1, 'Partial', NULL, 'customer', 'Prospect')

    PRINT 'Permission CLPCONF005 added';
END
ELSE
BEGIN
    PRINT 'Permission CLPCONF005 already exists';
END
GO

-- CLPCONF006: Accéder à la section Lettre de mission
IF (SELECT COUNT(*) FROM [auth].[Authorization] WHERE Code = 'CLPCONF006') = 0
BEGIN
    INSERT INTO [auth].[Authorization] (
        [Name], [Description], [Code], [Label], [Category], [Configurable], [View], [ProductCode], [Type], [TargetAccountType])
    VALUES
        ('Access Mission Letter Section', '', 'CLPCONF006', 'Accéder à la section Lettre de mission', 'CLTPCONF', 1, 'Partial', NULL, 'customer', 'Prospect')

    PRINT 'Permission CLPCONF006 added';
END
ELSE
BEGIN
    PRINT 'Permission CLPCONF006 already exists';
END
GO

-- CLPDOCP001: Accéder au dossier Documents
IF (SELECT COUNT(*) FROM [auth].[Authorization] WHERE Code = 'CLPDOCP001') = 0
BEGIN
    INSERT INTO [auth].[Authorization] (
        [Name], [Description], [Code], [Label], [Category], [Configurable], [View], [ProductCode], [Type], [TargetAccountType])
    VALUES
        ('Access Documents Folder', '', 'CLPDOCP001', 'Accéder au dossier Documents', 'CLTPDOCP', 1, 'Partial', NULL, 'customer', 'Prospect')

    PRINT 'Permission CLPDOCP001 added';
END
ELSE
BEGIN
    PRINT 'Permission CLPDOCP001 already exists';
END
GO

-- =============================================
-- Update Administration permissions to TargetAccountType = 'All'
-- =============================================

-- Update customer admin permissions to All
UPDATE [auth].[Authorization]
SET [TargetAccountType] = 'All'
WHERE [Code] IN ('CLUSER001', 'CLUSER002', 'CLUSER003', 'CLUSER004', 'CLINFO001')
  AND [TargetAccountType] = 'Client';

PRINT 'Updated customer admin permissions (CLUSER001-004, CLINFO001) to All';
GO
