using System.Text;
using System.Text.Json;
using AuthService.AppSettingsOptions;
using AuthService.Common.Dtos.MessageBusDtos;
using RabbitMQ.Client;

namespace AuthService.AsyncDataServices;
public class MessageBusClient : IMessageBusClient
{
    private readonly RabbitMQOptions _options;

    private readonly IConnection? _connection;
    private readonly IModel? _channel;

    public MessageBusClient(RabbitMQOptions options)
    {
        _options = options;

        var factory = new ConnectionFactory()
        {
            HostName = _options.Host,
            Port = int.Parse(_options.Port),
            ClientProvidedName = _options.ClientProvidedName,
            UserName = _options.UserName,
            Password = _options.Password,
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(_options.Exchange, ExchangeType.Direct);
            _channel.QueueDeclare(_options.QueueName, false, false, false, null);
            _channel.QueueBind(_options.QueueName, _options.Exchange, _options.RoutingKey, null);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"--> Could not connect to the message bus: {ex.Message}");
        }
    }

    public void PublishUser(PublishUserDto dto)
    {
        var message = JsonSerializer.Serialize(dto);

        SendMessage(message);
    }

    public void Dispose()
    {
        if (_channel != null && _channel.IsOpen)
        {
            _channel.Close();
            _connection?.Close();
        }
    }

    private void SendMessage(string message)
    {
        if (_connection != null && _connection.IsOpen)
        {
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                _options.Exchange,
                _options.RoutingKey,
                basicProperties: null,
                body
            );
        }
    }

    private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> RabbitMQ connection shutdown");
    }
}