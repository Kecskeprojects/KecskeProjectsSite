namespace Backend.Communication.Outgoing;

public class GenericResponse
{
    public string? Error { get; private set; }
    public string? Message { get; private set; }

    public GenericResponse()
    {

    }

    public GenericResponse(string error, bool isError = false)
    {
        if (isError)
        {
            Error = error;
        }
        else
        {
            Message = error;

        }
    }
}

public class GenericResponse<T> : GenericResponse
{
    public T? Content { get; private set; }

    public GenericResponse(string error, bool isError = false) : base(error, isError)
    {
    }


    public GenericResponse(T data)
    {
        Content = data;
    }
}
