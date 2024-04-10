CREATE TABLE [actor].[Contact] (
    [ContactId]             INT              IDENTITY (1, 1) NOT NULL,
    [ContactGlobalUniqueId] UNIQUEIDENTIFIER NOT NULL,
    [FirstName]             VARCHAR (50)     NOT NULL,
    [LastName]              VARCHAR (50)     NOT NULL,
    [ContactEmail]          VARCHAR (150)    NOT NULL,
    [PersonnaName]          VARCHAR (50)     NULL,
    [Type]                  VARCHAR (20)     NOT NULL,
    [Status]                VARCHAR (20)     NULL,
    CONSTRAINT [C_TContact_PK] PRIMARY KEY CLUSTERED ([ContactId] ASC)
);



