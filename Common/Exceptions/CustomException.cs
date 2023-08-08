namespace AuthService.Common.Exceptions;
public abstract class CustomException : Exception
{
    public virtual int StatusCode => 400;

    public CustomException()
    {
    }

    public CustomException(string? message)
        : base(message)
    {
    }

    public CustomException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}