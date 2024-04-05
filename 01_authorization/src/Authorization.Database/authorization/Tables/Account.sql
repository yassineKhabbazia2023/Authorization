CREATE TABLE [authorization].[Account] (
    [AccountId]               INT              IDENTITY (1, 1) NOT NULL,
    [AccountGlobalUniqueId]   UNIQUEIDENTIFIER NOT NULL,
    [AccountNumber]           VARCHAR (100)    NOT NULL,
    [LegalName]               NVARCHAR (255)   NOT NULL,
    [IsActive]                BIT              NOT NULL,
    CONSTRAINT [C_Account_PK] PRIMARY KEY CLUSTERED ([AccountId] ASC)
);

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''identifiant technique',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'AccountId'
	
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La raison social de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'LegalName'

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'La source de création de l''entité',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'AccountNumber'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'L''entité est-elle activé',
    @level0type = N'SCHEMA',
    @level0name = N'account',
    @level1type = N'TABLE',
    @level1name = N'Account',
    @level2type = N'COLUMN',
    @level2name = N'IsActive'
GO
