CREATE TABLE [auth].[ContactAuthorization] (
    [ContactId]    INT NOT NULL,
    [AccountId]    INT NOT NULL,
    [AuthorizationId]     INT NOT NULL,
    [CreationDate] DATETIME NOT NULL,
    CONSTRAINT [PK_ContactAuthorization] PRIMARY KEY CLUSTERED ([ContactId] ASC, [AccountId] ASC, [AuthorizationId] ASC),
    CONSTRAINT [FK_ContactAuthorization_Account] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId]),
    CONSTRAINT [FK_ContactAuthorization_Authorization] FOREIGN KEY ([AuthorizationId]) REFERENCES [auth].[Authorization] ([AuthorizationId]),
    CONSTRAINT [FK_ContactAuthorization_Contact] FOREIGN KEY ([ContactId]) REFERENCES [actor].[Contact] ([ContactId])
);

