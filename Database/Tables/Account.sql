CREATE TABLE [dbo].[Account]
(
	[AccountId] INT NOT NULL IDENTITY, 
    [Email] NVARCHAR(100) NOT NULL, 
    [Password] VARBINARY(84) NOT NULL, 
    [UserName] NVARCHAR(100) NOT NULL,
    [LastLoginUtc] DATETIME NULL, 
    [CreatedOnUtc] DATETIME NOT NULL CONSTRAINT [DF_Account_CreatedOnUtc] DEFAULT GETUTCDATE(), 
    [ModifiedOnUtc] DATETIME NOT NULL CONSTRAINT [DF_Account_ModifiedOnUtc] DEFAULT GETUTCDATE(), 
    CONSTRAINT [PK_Account_AccountId] PRIMARY KEY ([AccountId]),
    CONSTRAINT [UQ_Account_Email] UNIQUE ([Email])
)
