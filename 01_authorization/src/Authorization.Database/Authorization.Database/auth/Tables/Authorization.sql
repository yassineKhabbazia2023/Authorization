CREATE TABLE [auth].[Authorization] (
    [ContactId]    INT NOT NULL,
    [AccountId]    INT NOT NULL,
    [ActionId]     INT NOT NULL,
    [CreationDate] DATETIME NOT NULL,
    CONSTRAINT [PK_Authorization] PRIMARY KEY CLUSTERED ([ContactId] ASC, [AccountId] ASC, [ActionId] ASC),
    CONSTRAINT [FK_Authorization_Account] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId]),
    CONSTRAINT [FK_Authorization_Action] FOREIGN KEY ([ActionId]) REFERENCES [auth].[Action] ([ActionId]),
    CONSTRAINT [FK_Authorization_Contact] FOREIGN KEY ([ContactId]) REFERENCES [actor].[Contact] ([ContactId])
);

