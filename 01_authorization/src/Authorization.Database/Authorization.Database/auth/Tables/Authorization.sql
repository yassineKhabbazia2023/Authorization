CREATE TABLE [auth].[Authorization] (
    [ContactId]    INT NOT NULL,
    [AccountId]    INT NOT NULL,
    [ActionId]     INT NOT NULL,
    [CreationDate] INT NULL,
    CONSTRAINT [PK_Permission_1] PRIMARY KEY CLUSTERED ([ContactId] ASC, [AccountId] ASC, [ActionId] ASC),
    CONSTRAINT [FK_Permission_Account] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId]),
    CONSTRAINT [FK_Permission_Action] FOREIGN KEY ([ActionId]) REFERENCES [auth].[Action] ([ActionId]),
    CONSTRAINT [FK_Permission_Contact] FOREIGN KEY ([ContactId]) REFERENCES [actor].[Contact] ([ContactId])
);

