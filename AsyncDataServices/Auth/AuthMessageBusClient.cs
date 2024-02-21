using System.Text;
using System.Text.Json;
using AuthService.AppSettingsOptions;
using AuthService.Common.Dtos.MessageBusDtos;
using RabbitMQ.Client;

namespace AuthService.AsyncDataServices.Auth;
public class AuthMessageBusClient : IAuthMessageBusClient
{
    private readonly RabbitMQOptions _options;
    private readonly ILogger<AuthMessageBusClient> _logger;

    private readonly IConnection? _connection;
    private readonly IModel? _channel;

    public AuthMessageBusClient(RabbitMQOptions options, ILogger<AuthMessageBusClient> logger)
    {
        _options = options;
        _logger = logger;

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

            _channel.ExchangeDeclare(_options.Auth.Exchange, ExchangeType.Direct);
            _channel.QueueDeclare(_options.Auth.QueueName, false, false, false, null);
            _channel.QueueBind(_options.Auth.QueueName, _options.Auth.Exchange, _options.Auth.RoutingKey, null);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }
        catch (Exception ex)
        {
            _logger.LogError($"--> Could not connect to the message bus: {ex.Message}");
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

            _channel?.BasicPublish(
                _options.Auth.Exchange,
                _options.Auth.RoutingKey,
                basicProperties: null,
                body
            );
        }
        else
        {
            _logger.LogWarning($"Message has not been sent because connection does not exist or is not open");
        }
    }

    private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        _logger.LogInformation($"RabbitMQ connection shutdown");
    }
}