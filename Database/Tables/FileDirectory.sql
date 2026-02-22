CREATE TABLE [dbo].[FileDirectory]
(
	[FileDirectoryId] INT NOT NULL IDENTITY, 
    [RelativePath] NVARCHAR(200) NOT NULL, 
    [DisplayName] NVARCHAR(200) NOT NULL, 
    [CreatedOnUtc] DATETIME NOT NULL CONSTRAINT [DF_FileDirectory_CreatedOnUtc] DEFAULT GETUTCDATE(), 
    [ModifiedOnUtc] DATETIME NOT NULL CONSTRAINT [DF_FileDirectory_ModifiedOnUtc] DEFAULT GETUTCDATE(), 
    CONSTRAINT [PK_FileDirectory_FileDirectoryId] PRIMARY KEY ([FileDirectoryId]),
    CONSTRAINT [UQ_FileDirectory_Name] UNIQUE ([RelativePath])
)
