namespace AuthService.Common.Dtos.MessageBusDtos
{
    public class VerifyEmailDto : IMessageBusDto
    {
        public string Email { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
        public string Event => "Verify_Email";
    }
}
