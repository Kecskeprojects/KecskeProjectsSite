CREATE TABLE [dbo].[Account]
(
	[AccountId] INT NOT NULL IDENTITY, 
    [Password] VARBINARY(84) NOT NULL, 
    [UserName] NVARCHAR(200) NOT NULL,
    [LastLoginOnUtc] DATETIME NOT NULL CONSTRAINT [DF_Account_LastLoginOnUtc] DEFAULT GETUTCDATE(), 
    [CreatedOnUtc] DATETIME NOT NULL CONSTRAINT [DF_Account_CreatedOnUtc] DEFAULT GETUTCDATE(), 
    [ModifiedOnUtc] DATETIME NOT NULL CONSTRAINT [DF_Account_ModifiedOnUtc] DEFAULT GETUTCDATE(), 
    CONSTRAINT [PK_Account_AccountId] PRIMARY KEY ([AccountId]),
    CONSTRAINT [UQ_Account_UserName] UNIQUE ([UserName])
)
