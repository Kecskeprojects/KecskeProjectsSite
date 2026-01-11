CREATE TABLE [dbo].[PermittedIpAddress]
(
	[PermittedIpAddressId] INT NOT NULL IDENTITY, 
	[AccountId] INT NOT NULL,
    [IpAddress] VARCHAR(15) NOT NULL,
	[ExpiresOnUtc] DATETIME NOT NULL,
	[CreatedOnUtc] DATETIME NOT NULL CONSTRAINT [DF_PermittedIpAddress_CreatedOnUtc] DEFAULT GETUTCDATE(),
	[ModifiedOnUtc] DATETIME NOT NULL CONSTRAINT [DF_PermittedIpAddress_ModifiedOnUtc] DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_PermittedIpAddress_PermittedIpAddressId] PRIMARY KEY ([PermittedIpAddressId]),
    CONSTRAINT [FK_PermittedIpAddress_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Account] ([AccountId])
)
