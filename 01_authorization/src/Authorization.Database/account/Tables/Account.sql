CREATE TABLE [account].[Account] (
    [AccountId]             INT              NOT NULL,
    [AccountGlobalUniqueId] UNIQUEIDENTIFIER NULL,
    [AccountNumber]         VARCHAR (100)     NOT NULL,
    [LegalName]             NVARCHAR (255)   NOT NULL,
    [Status]                VARCHAR(20)      NULL,
    [CreationDate]          DATETIME2        NOT NULL DEFAULT GETDATE(),
    [LastUpdateDate]        DATETIME2        NULL,
    CONSTRAINT [C_TAccount_PK] PRIMARY KEY CLUSTERED ([AccountId] ASC)
);

