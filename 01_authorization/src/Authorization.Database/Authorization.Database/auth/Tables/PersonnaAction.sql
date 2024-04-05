CREATE TABLE [auth].[PersonnaAction] (
    [PersonnaId] INT NOT NULL,
    [ActionId]   INT NOT NULL,
    CONSTRAINT [PK_PersonnaAction] PRIMARY KEY CLUSTERED ([PersonnaId] ASC, [ActionId] ASC),
    CONSTRAINT [FK_PersonnaAction_Action] FOREIGN KEY ([ActionId]) REFERENCES [auth].[Action] ([ActionId]),
    CONSTRAINT [FK_PersonnaAction_Personna] FOREIGN KEY ([PersonnaId]) REFERENCES [auth].[Personna] ([PersonnaId])
);

