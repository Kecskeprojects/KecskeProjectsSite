CREATE TRIGGER [FileFolderUpdated]
	ON [dbo].[FileFolder]
	AFTER UPDATE
	AS
	BEGIN
		SET NOCOUNT ON

		UPDATE [dbo].[FileFolder]
		SET [ModifiedOnUtc] = GETUTCDATE()
		FROM Inserted [i]
		WHERE [dbo].[FileFolder].[FileFolderId] = [i].[FileFolderId]
	END
