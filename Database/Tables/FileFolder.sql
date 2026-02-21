CREATE TABLE [dbo].[FileFolder]
(
	[FileFolderId] INT NOT NULL IDENTITY, 
    [RelativePath] NVARCHAR(200) NOT NULL, 
    [DisplayName] NVARCHAR(200) NOT NULL, 
    [CreatedOnUtc] DATETIME NOT NULL CONSTRAINT [DF_Folder_CreatedOnUtc] DEFAULT GETUTCDATE(), 
    [ModifiedOnUtc] DATETIME NOT NULL CONSTRAINT [DF_Folder_ModifiedOnUtc] DEFAULT GETUTCDATE(), 
    CONSTRAINT [PK_FileFolder_FileFolderId] PRIMARY KEY ([FileFolderId]),
    CONSTRAINT [UQ_FileFolder_Name] UNIQUE ([RelativePath])
)
