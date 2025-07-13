CREATE TABLE [dbo].[AccountRole]
(
	[AccountId] INT NOT NULL,
    [RoleId] INT NOT NULL,
    CONSTRAINT [FK_AccountRole_Server] FOREIGN KEY ([RoleId]) REFERENCES [Role]([RoleId]),
    CONSTRAINT [FK_AccountRole_Account] FOREIGN KEY ([AccountId]) REFERENCES [Account]([AccountId]),
    CONSTRAINT [PK_AccountRole_RoleId_AccountId] PRIMARY KEY ([RoleId], [AccountId])
)
