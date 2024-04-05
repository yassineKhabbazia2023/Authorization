CREATE TABLE [auth].[Ressource] (
    [RessourceId]       INT           IDENTITY (1, 1) NOT NULL,
    [RessourceName]     VARCHAR (100) NULL,
    [RessourceParentId] INT           NULL,
    CONSTRAINT [PK_Ressource] PRIMARY KEY CLUSTERED ([RessourceId] ASC),
    CONSTRAINT [FK_Permission_Premission] FOREIGN KEY ([RessourceParentId]) REFERENCES [auth].[Ressource] ([RessourceId])
);

