using AuthService.Common.Dtos.MessageBusDtos;

namespace AuthService.AsyncDataServices.Interfaces
{
    public interface IMessageBusClient : IDisposable
    {
        void SendMessage<T>(T data)
            where T : class, IMessageBusDto;
    }
}
