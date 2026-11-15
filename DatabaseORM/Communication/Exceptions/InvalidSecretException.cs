namespace DatabaseORM.Communication.Exceptions;

/// <summary>
/// A secret the user supplied, e.g. the password reset key, doesn't match the stored hash (400).
/// Deliberately not 401: the frontend treats a 401 as an expired session and redirects to the login page.
/// </summary>
public class InvalidSecretException(string message) : ServiceException(message);
