CREATE TABLE [auth].[Permission] (
    [PermissionId] INT IDENTITY (1, 1) NOT NULL,
    [ContactId]    INT NULL,
    [AccountId]    INT NULL,
    [ActionId]     INT NULL,
    [CreationDate] INT NULL,
    CONSTRAINT [PK_Permission] PRIMARY KEY CLUSTERED ([PermissionId] ASC),
    CONSTRAINT [FK_Permission_Account] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId]),
    CONSTRAINT [FK_Permission_Action] FOREIGN KEY ([ActionId]) REFERENCES [auth].[Action] ([ActionId]),
    CONSTRAINT [FK_Permission_Contact] FOREIGN KEY ([ContactId]) REFERENCES [actor].[Contact] ([ContactId])
);

