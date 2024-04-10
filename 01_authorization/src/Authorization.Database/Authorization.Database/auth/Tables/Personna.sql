CREATE TABLE [auth].[Personna] (
    [PersonnaId] INT           NOT NULL,
    [Name]       VARCHAR (100) NULL,
    [Type]       VARCHAR (20)  NULL,
    CONSTRAINT [PK_Personna] PRIMARY KEY CLUSTERED ([PersonnaId] ASC)
);



