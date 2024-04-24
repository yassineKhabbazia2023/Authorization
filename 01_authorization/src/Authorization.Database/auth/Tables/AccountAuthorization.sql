CREATE TABLE [auth].[AccountAuthorization] (
    [AccountId]  INT NOT NULL,
    [AuthorizationId] INT NOT NULL,
    CONSTRAINT [PK_AccountAuthorization] PRIMARY KEY CLUSTERED ([AccountId] ASC, [AuthorizationId] ASC),
    CONSTRAINT [FK_AccountAuthorization_Authorization] FOREIGN KEY ([AuthorizationId]) REFERENCES [auth].[Authorization] ([AuthorizationId]),
    CONSTRAINT [FK_AccountAuthorization_Account] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId])
);

