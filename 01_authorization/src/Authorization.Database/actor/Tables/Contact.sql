CREATE TABLE [actor].[Contact] (
    [ContactId]             INT              IDENTITY (1, 1) NOT NULL,
    [ContactGlobalUniqueId] UNIQUEIDENTIFIER NOT NULL,
    [FirstName]             VARCHAR (50)     NOT NULL,
    [LastName]              VARCHAR (50)     NOT NULL,
    [ContactEmail]          VARCHAR (50)     NOT NULL,
    [PersonnaName]          VARCHAR (30)     NULL,
    [Type]                  VARCHAR (20)     NOT NULL,
    CONSTRAINT [C_TContact_PK] PRIMARY KEY CLUSTERED ([ContactId] ASC)
);

