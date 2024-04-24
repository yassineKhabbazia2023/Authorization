CREATE TABLE [auth].[Authorization] (
    [AuthorizationId]    INT           IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (100) NOT NULL,
    [Description] VARCHAR (250) NOT NULL,
    [Code]        VARCHAR (10)  NOT NULL,
    [Label]       VARCHAR (100)  NOT NULL,
    [Category]    VARCHAR (50) NULL,
    [Configurable] BIT
    CONSTRAINT [PK_Authorization] PRIMARY KEY CLUSTERED ([AuthorizationId] ASC)
);



