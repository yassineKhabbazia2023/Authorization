CREATE TABLE [actor].[Contact] (
    [ContactId]             INT              NOT NULL,
    [ContactGlobalUniqueId] UNIQUEIDENTIFIER NULL,
    [FirstName]             VARCHAR (100)    NOT NULL,
    [LastName]              VARCHAR (100)    NOT NULL,
    [Email]                 VARCHAR (200)    NOT NULL,
    [Type]                  VARCHAR (20)     NOT NULL,
    [Status]                VARCHAR (20)     NULL,
    [PersonaName]           VARCHAR (50)     NOT NULL,
    [CreationDate]          DATETIME2        NOT NULL DEFAULT GETDATE(),
    [LastUpdateDate]        DATETIME2        NULL,
    [IsActive]              BIT              NOT NULL DEFAULT(1)
    CONSTRAINT [C_TContact_PK] PRIMARY KEY CLUSTERED ([ContactId] ASC)
)
GO
CREATE NONCLUSTERED INDEX [IX_Contact_IsActive]
    ON  [actor].[Contact]([IsActive] ASC);



