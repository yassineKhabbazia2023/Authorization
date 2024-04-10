CREATE TABLE [account].[Account] (
    [AccountId]             INT              NOT NULL,
    [AccountGlobalUniqueId] UNIQUEIDENTIFIER NOT NULL,
    [AccountNumber]         VARCHAR (100)    NOT NULL,
    [LegalName]             NVARCHAR (255)   NOT NULL,
    [Email]                 NVARCHAR (100)   NOT NULL,
    [IsActive]              BIT              NOT NULL,
    CONSTRAINT [C_TAccount_PK] PRIMARY KEY CLUSTERED ([AccountId] ASC)
);

