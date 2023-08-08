namespace AuthService.Common.Exceptions
{
    public class AccessDeniedException : CustomException
    {
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
