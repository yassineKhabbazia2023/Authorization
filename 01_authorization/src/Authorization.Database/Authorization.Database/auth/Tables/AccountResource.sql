CREATE TABLE [auth].[AccountResource] (
    [AccountId]  INT NOT NULL,
    [ResourceId] INT NOT NULL,
    [IsActive] BIT NULL, 
    CONSTRAINT [PK_AccountResource] PRIMARY KEY CLUSTERED ([AccountId] ASC, [ResourceId] ASC),
    CONSTRAINT [FK_AccountResource_Resource] FOREIGN KEY ([ResourceId]) REFERENCES [auth].[Resource] ([ResourceId]),
    CONSTRAINT [FK_AccountResource_Account] FOREIGN KEY ([AccountId]) REFERENCES [account].[Account] ([AccountId])
);

