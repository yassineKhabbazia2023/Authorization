CREATE TABLE [auth].[Personna] (
    [PersonnaId]   INT          NOT NULL,
    [PersonnaName] VARCHAR (50) NULL,
    [Type]         VARCHAR (20) NULL,
    CONSTRAINT [PK_Personna] PRIMARY KEY CLUSTERED ([PersonnaId] ASC)
);

