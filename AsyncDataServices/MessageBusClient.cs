using System.Text;
using System.Text.Json;
using AuthService.Common.Constants;
using AuthService.Common.Dtos.MessageBusDtos;
using RabbitMQ.Client;

namespace AuthService.AsyncDataServices;
public class MessageBusClient : IMessageBusClient
{
    private readonly IConfiguration _configuration;
    private readonly IConnection? _connection;
    private readonly IModel? _channel;

    public MessageBusClient(IConfiguration configuration)
    {
        _configuration = configuration;
        var factory = new ConnectionFactory()
        {
            HostName = _configuration[AppConstants.RabbitMQHost],
            Port = int.Parse(_configuration[AppConstants.RabbitMQPort]!)
        };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

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
                exchange: "trigger",
                routingKey: "",
                basicProperties: null,
                body: body
            );
        }
    }

    private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        Console.WriteLine("--> RabbitMQ connection shutdown");
    }
}