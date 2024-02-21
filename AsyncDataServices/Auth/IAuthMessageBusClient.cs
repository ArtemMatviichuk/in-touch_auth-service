using AuthService.Common.Dtos.MessageBusDtos;

namespace AuthService.AsyncDataServices.Auth;
public interface IAuthMessageBusClient
{
    void PublishUser(PublishUserDto dto);
}