CREATE TABLE [auth].[Resource] (
    [ResourceId] INT           IDENTITY (1, 1) NOT NULL,
    [ParentId]   INT           NULL,
    [Name]       VARCHAR (100) NOT NULL,
    [Label]       VARCHAR (100) NOT NULL,
    [Category]       VARCHAR (20)  NOT NULL,
    [Code]       VARCHAR (20)  NOT NULL,
    [OrderShow] INT NULL, 
    [Type] VARCHAR(50) NOT NULL, 
    CONSTRAINT [PK_Ressource] PRIMARY KEY CLUSTERED ([ResourceId] ASC),
    CONSTRAINT [FK_Authorization_Authorization] FOREIGN KEY ([ParentId]) REFERENCES [auth].[Resource] ([ResourceId]),
    CONSTRAINT [CHK_Category] CHECK (Category = 'unitaire' OR Category = 'globale')
);

