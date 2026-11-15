namespace DatabaseORM.Communication.Exceptions;

/// <summary>
/// The entity the operation needs doesn't exist (404).
/// </summary>
public class NotFoundException(string message) : ServiceException(message);
