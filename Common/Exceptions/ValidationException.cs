namespace AuthService.Common.Exceptions
{
    public class ValidationException : CustomException
    {
        public ValidationException()
        {
        }

        public ValidationException(string? message)
            : base(message)
        {
        }

        public ValidationException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
