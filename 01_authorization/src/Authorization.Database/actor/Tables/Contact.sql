CREATE TABLE [actor].[Contact] (
    [ContactId]             INT              IDENTITY (1, 1) NOT NULL,
    [ContactGlobalUniqueId] UNIQUEIDENTIFIER NULL,
    [FirstName]             VARCHAR (100)     NOT NULL,
    [LastName]              VARCHAR (100)     NOT NULL,
    [Email]          VARCHAR (200)    NOT NULL,
    [Type]          VARCHAR (20)     NOT NULL,
    [Status]                  VARCHAR (20)     NULL,
    [PersonaName]                VARCHAR (50)     NOT NULL,
    [CreationDate] DATETIME2 NULL, 
    CONSTRAINT [C_TContact_PK] PRIMARY KEY CLUSTERED ([ContactId] ASC),
    CONSTRAINT [CHK_Type] CHECK (Type = 'Collaborator' OR Type = 'Customer'),
    CONSTRAINT [CHK_Status] CHECK (Status = 'Connected' OR Status = 'Declared' OR Status = 'Invited' OR Status = 'Removed')
);



