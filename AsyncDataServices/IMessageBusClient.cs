using AuthService.Common.Dtos.MessageBusDtos;

namespace AuthService.AsyncDataServices;
public interface IMessageBusClient
{
    void PublishUser(PublishUserDto dto);
}