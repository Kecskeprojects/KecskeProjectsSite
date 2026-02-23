CREATE TRIGGER [PermittedIpAddressUpdated]
	ON [dbo].[PermittedIpAddress]
	AFTER UPDATE
	AS
	BEGIN
		SET NOCOUNT ON

		UPDATE [dbo].[PermittedIpAddress]
		SET [ModifiedOnUtc] = GETUTCDATE()
		FROM Inserted [i]
		WHERE [dbo].[PermittedIpAddress].[PermittedIpAddressId] = [i].[PermittedIpAddressId]
	END
