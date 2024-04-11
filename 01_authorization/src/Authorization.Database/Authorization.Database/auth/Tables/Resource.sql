CREATE TABLE [auth].[Resource] (
    [ResourceId] INT           IDENTITY (1, 1) NOT NULL,
    [ParentId]   INT           NULL,
    [Name]       VARCHAR (100) NULL,
    [Code]       VARCHAR (20)  NULL,
    [Url]        VARCHAR (500) NULL,
    [Visible]    BIT           NULL,
    CONSTRAINT [PK_Ressource] PRIMARY KEY CLUSTERED ([ResourceId] ASC),
    CONSTRAINT [FK_Permission_Premission] FOREIGN KEY ([ParentId]) REFERENCES [auth].[Resource] ([ResourceId])
);

