using AuthService.AppSettingsOptions;
using AuthService.AsyncDataServices.Interfaces;
using AuthService.Common.Dtos.MessageBusDtos;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AuthService.AsyncDataServices.Implementations
{
    public abstract class MessageBusClient : IMessageBusClient
    {
        protected readonly IRabbitMqConnection _connection;
        public readonly ExchangeOptions _options;
        protected readonly ILogger _logger;

        protected readonly IModel _channel;

        protected MessageBusClient(IRabbitMqConnection connection, ExchangeOptions options, ILogger logger)
        {
            _connection = connection;
            _options = options;
            _logger = logger;

            _channel = _connection.CreateChannel(_options);
        }

        public void SendMessage<T>(T data)
            where T : class, IMessageBusDto
        {
            if (!_connection.IsOpen())
            {
                throw new Exception($"Message has not been sent because connection does not exist or is not open");
            }

            var message = JsonSerializer.Serialize(data);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: _options.Exchange,
                routingKey: _options.IsFanout ? string.Empty : _options.RoutingKey,
                basicProperties: null,
                body: body);
        }

        public void Dispose()
        {
            if (_channel != null && _channel.IsOpen)
            {
                _channel.Close();
            }
        }
    }
}
