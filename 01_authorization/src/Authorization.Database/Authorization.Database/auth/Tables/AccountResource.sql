CREATE TABLE [auth].[AccountResource] (
    [AccountId]  INT NOT NULL,
    [ResourceId] INT NOT NULL,
    CONSTRAINT [PK_AccountResource] PRIMARY KEY CLUSTERED ([AccountId] ASC, [ResourceId] ASC)
);

