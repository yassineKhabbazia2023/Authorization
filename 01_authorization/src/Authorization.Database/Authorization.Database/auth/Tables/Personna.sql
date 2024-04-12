CREATE TABLE [auth].[Personna] (
    [PersonnaId] INT           NOT NULL,
    [Name]       VARCHAR (100) NOT NULL,
    [Type]       VARCHAR (20)  NOT NULL,
    CONSTRAINT [PK_Personna] PRIMARY KEY CLUSTERED ([PersonnaId] ASC)
);



