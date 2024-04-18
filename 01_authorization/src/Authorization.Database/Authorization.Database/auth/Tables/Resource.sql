CREATE TABLE [auth].[Resource] (
    [ResourceId] INT           IDENTITY (1, 1) NOT NULL,
    [ParentId]   INT           NULL,
    [Name]       VARCHAR (100) NOT NULL,
    [Label]       VARCHAR (100) NOT NULL,
    [Category]       VARCHAR (20)  NOT NULL,
    [Code]       VARCHAR (20)  NOT NULL,
    [OrderShow] INT NULL, 
    CONSTRAINT [PK_Ressource] PRIMARY KEY CLUSTERED ([ResourceId] ASC),
    CONSTRAINT [FK_Permission_Premission] FOREIGN KEY ([ParentId]) REFERENCES [auth].[Resource] ([ResourceId]),
    CONSTRAINT [CHK_Category] CHECK (Category = 'navigation' OR Category = 'globale')
);

