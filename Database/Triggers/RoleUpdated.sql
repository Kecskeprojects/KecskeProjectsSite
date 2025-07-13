CREATE TRIGGER [RoleUpdated]
	ON [dbo].[Role]
	AFTER UPDATE
	AS
	BEGIN
		SET NOCOUNT ON

		UPDATE [dbo].[Role]
		SET [ModifiedOnUtc] = GETUTCDATE()
		FROM Inserted [i]
		WHERE [dbo].[Role].[RoleId] = [i].[RoleId]
	END
