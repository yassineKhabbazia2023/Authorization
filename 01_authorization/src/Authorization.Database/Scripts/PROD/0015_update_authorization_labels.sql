UPDATE
    [auth].[Authorization]
SET
    [label] = CASE
        WHEN [code] = 'CLGED0001' THEN 'Accéder à l’espace documentaire Gestion Sociale'
        WHEN [code] = 'CLGED0002' THEN 'Accéder à l’espace documentaire Comptabilité'
        WHEN [code] = 'CLSILA001' THEN 'Accéder à l’outil Silae'
        WHEN [code] = 'CLMEG001' THEN 'Accéder à l’outil MEG'
        WHEN [code] = 'CLPEN001' THEN 'Accéder à l’outil Pennylane'
        WHEN [code] = 'CLRAPP001' THEN 'Accéder au rapport BI Financier'
    END
WHERE
    [code] in (
        'CLGED0001',
        'CLGED0002',
        'CLMEG001',
        'CLPEN001',
        'CLRAPP001',
        'CLSILA001'
    );