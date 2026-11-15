namespace DatabaseORM.Communication.Exceptions;

/// <summary>
/// An expected outcome of a service call that the user should read, so its message must be safe to show to the client.
/// The backend's ErrorLoggingFilterAttribute turns each subclass into a response with a matching status code.
/// </summary>
public abstract class ServiceException(string message) : Exception(message);
