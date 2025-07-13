CREATE TRIGGER [AccountUpdated]
	ON [dbo].[Account]
	AFTER UPDATE
	AS
	BEGIN
		SET NOCOUNT ON

		UPDATE [dbo].[Account]
		SET [ModifiedOnUtc] = GETUTCDATE()
		FROM Inserted [i]
		WHERE [dbo].[Account].[AccountId] = [i].[AccountId]
	END
