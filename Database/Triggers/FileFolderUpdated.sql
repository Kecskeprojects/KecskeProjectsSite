CREATE TRIGGER [FileDirectoryUpdated]
	ON [dbo].[FileDirectory]
	AFTER UPDATE
	AS
	BEGIN
		SET NOCOUNT ON

		UPDATE [dbo].[FileDirectory]
		SET [ModifiedOnUtc] = GETUTCDATE()
		FROM Inserted [i]
		WHERE [dbo].[FileDirectory].[FileDirectoryId] = [i].[FileDirectoryId]
	END
