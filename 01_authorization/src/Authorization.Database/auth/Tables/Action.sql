CREATE TABLE [auth].[Action] (
    [ActionId]    INT           IDENTITY (1, 1) NOT NULL,
    [RessourceId] INT           NULL,
    [ActionName]  VARCHAR (100) NOT NULL,
    [ActionCode]  VARCHAR (10)  NOT NULL,
    CONSTRAINT [PK_Action] PRIMARY KEY CLUSTERED ([ActionId] ASC),
    CONSTRAINT [FK_Action_Ressource] FOREIGN KEY ([RessourceId]) REFERENCES [auth].[Ressource] ([RessourceId])
);

