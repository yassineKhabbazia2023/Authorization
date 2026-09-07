-- =============================================
-- Script: Add CLRAPNM001 permission
-- Date: 2026-09-07
-- Description: Add the customer permission to access the monthly accounting note.
--              ProductCode = 'monthlynote' triggers automatic assignment on Pennylane offer activation.
-- =============================================

IF (SELECT COUNT(*) FROM [auth].[Authorization] WHERE Code = 'CLRAPNM001') = 0
BEGIN
    INSERT INTO [auth].[Authorization] (
        [Name], [Description], [Code], [Label], [Category], [Configurable], [View], [ProductCode], [Type], [TargetAccountType])
    VALUES
        ('View monthly note', '', 'CLRAPNM001', 'Accéder à la note mensuelle comptable', 'CLTPILOTAGE', 1, 'Partial', 'monthlynote', 'customer', 'Client')

    PRINT 'Permission CLRAPNM001 added';
END
ELSE
BEGIN
    PRINT 'Permission CLRAPNM001 already exists';
END
GO
