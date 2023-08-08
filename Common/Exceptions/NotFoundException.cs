namespace AuthService.Common.Exceptions;
public class NotFoundException : CustomException
{
    public override int StatusCode => 404;

    public NotFoundException(string? message)
        : base(message)
    {
    }

    public NotFoundException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

}