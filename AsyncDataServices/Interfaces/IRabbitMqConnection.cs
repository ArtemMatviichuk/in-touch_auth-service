using AuthService.AppSettingsOptions;
using RabbitMQ.Client;

namespace AuthService.AsyncDataServices.Interfaces
{
    public interface IRabbitMqConnection : IDisposable
    {
        bool IsOpen();
        IModel CreateChannel(ExchangeOptions options);
    }
}
