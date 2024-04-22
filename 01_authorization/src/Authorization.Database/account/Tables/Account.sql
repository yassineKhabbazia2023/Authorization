CREATE TABLE [account].[Account] (
    [AccountId]             INT              NOT NULL,
    [AccountGlobalUniqueId] UNIQUEIDENTIFIER NULL,
    [AccountNumber]         VARCHAR (20)    NOT NULL,
    [LegalName]             NVARCHAR (255)   NOT NULL,
    [Status]              VARCHAR(20)              NULL,
    CONSTRAINT [C_TAccount_PK] PRIMARY KEY CLUSTERED ([AccountId] ASC),
    CONSTRAINT [CHK_Status] CHECK (Status = 'ToDeploy' OR Status = 'InProgress' OR Status = 'Connected' OR Status = 'Revoked')
);

