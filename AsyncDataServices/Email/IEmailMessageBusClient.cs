using AuthService.Common.Dtos.MessageBusDtos;

namespace AuthService.AsyncDataServices.Auth;
public interface IEmailMessageBusClient
{
    void SendEmailConfirmationMessage(VerifyEmailDto dto);
}