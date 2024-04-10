CREATE TABLE [auth].[Action] (
    [ActionId]    INT           IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (100) NOT NULL,
    [Description] VARCHAR (255) NULL,
    [Code]        VARCHAR (10)  NULL,
    [Category]    VARCHAR (100) NULL,
    CONSTRAINT [PK_Action] PRIMARY KEY CLUSTERED ([ActionId] ASC)
);



