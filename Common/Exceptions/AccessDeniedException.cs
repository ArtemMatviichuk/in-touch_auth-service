namespace AuthService.Common.Exceptions
{
    public class AccessDeniedException : CustomException
    {
        public override int StatusCode => 403;

        public AccessDeniedException()
        {
        }

        public AccessDeniedException(string? message)
            : base(message)
        {
        }

        public AccessDeniedException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}
