CREATE TABLE [auth].[Action] (
    [ActionId]    INT           IDENTITY (1, 1) NOT NULL,
    [ResourceId]  INT NOT NULL,
    [Name]        VARCHAR (100) NOT NULL,
    [Description] VARCHAR (250) NOT NULL,
    [Code]        VARCHAR (10)  NOT NULL,
    [Category]    VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_Action] PRIMARY KEY CLUSTERED ([ActionId] ASC),
    CONSTRAINT [FK_Action_Resource] FOREIGN KEY ([ResourceId]) REFERENCES [auth].[Resource] ([ResourceId])
);



