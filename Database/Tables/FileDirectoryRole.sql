CREATE TABLE [dbo].[FileDirectoryRole]
(
	[FileDirectoryId] INT NOT NULL, 
    [RoleId] INT NOT NULL,
    CONSTRAINT [FK_FileDirectoryRole_FileDirectory] FOREIGN KEY ([FileDirectoryId]) REFERENCES [FileDirectory]([FileDirectoryId]),
    CONSTRAINT [FK_FileDirectoryRole_Role] FOREIGN KEY ([RoleId]) REFERENCES [Role]([RoleId]),
    CONSTRAINT [PK_FileDirectoryRole_RoleId_FileDirectory] PRIMARY KEY ([RoleId], [FileDirectoryId])
)
