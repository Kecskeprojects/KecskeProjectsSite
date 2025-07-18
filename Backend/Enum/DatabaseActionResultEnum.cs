namespace Backend.Enum;

public enum DatabaseActionResultEnum
{
    Failure = 0,
    Success = 1,
    PartialSuccess = 2,
    NotFound = 3,
    AlreadyExists = 4,

    FailureWithSpecialMessage = 100,
}
