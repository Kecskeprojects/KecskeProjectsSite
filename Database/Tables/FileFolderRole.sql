CREATE TABLE [dbo].[FileFolderRole]
(
	[FileFolderId] INT NOT NULL, 
    [RoleId] INT NOT NULL,
    CONSTRAINT [FK_FileFolderRole_FileFolder] FOREIGN KEY ([FileFolderId]) REFERENCES [FileFolder]([FileFolderId]),
    CONSTRAINT [FK_FileFolderRole_Role] FOREIGN KEY ([RoleId]) REFERENCES [Role]([RoleId]),
    CONSTRAINT [PK_FileFolderRole_RoleId_FileFolder] PRIMARY KEY ([RoleId], [FileFolderId])
)
