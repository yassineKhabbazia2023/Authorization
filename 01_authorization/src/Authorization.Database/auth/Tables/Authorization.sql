CREATE TABLE [auth].[Authorization] (
    [AuthorizationId]    INT           IDENTITY (1, 1) NOT NULL,
    [Name]        VARCHAR (100) NOT NULL,
    [Description] VARCHAR (250) NOT NULL,
    [Code]        VARCHAR (10)  NOT NULL,
    [Label]       VARCHAR (100)  NOT NULL,
    [View]    VARCHAR (10) NOT NULL,    
    [Category]    VARCHAR (50) NULL,
    [ProductCode] VARCHAR (20) NULL,
    [Configurable] BIT, 
    [Type] VARCHAR(15) NOT NULL
    CONSTRAINT [PK_Authorization] PRIMARY KEY CLUSTERED ([AuthorizationId] ASC)
    CONSTRAINT [CHK_Status] CHECK ([View] = 'Global' OR [View] = 'Partial' OR [View] = 'Both' OR [View] = 'Header')
    CONSTRAINT [CHK_Type] CHECK ([Type] = 'customer' OR [Type] = 'collaborator')
);

GO
CREATE NONCLUSTERED INDEX [IX_Authorization_View]
    ON [auth].[Authorization]([View] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Authorization_Type]
    ON [auth].[Authorization]([Type] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_Authorization_Code]
    ON [auth].[Authorization]([Code] ASC);

GO

