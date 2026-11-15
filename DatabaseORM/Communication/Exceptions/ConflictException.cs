namespace DatabaseORM.Communication.Exceptions;

/// <summary>
/// The operation would duplicate an existing entity, e.g. a taken username (409).
/// </summary>
public class ConflictException(string message) : ServiceException(message);
