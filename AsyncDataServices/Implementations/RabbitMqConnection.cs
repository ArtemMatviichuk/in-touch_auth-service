using AuthService.AppSettingsOptions;
using AuthService.AsyncDataServices.Interfaces;
using RabbitMQ.Client;

namespace AuthService.AsyncDataServices.Implementations
{
    public class RabbitMqConnection : IRabbitMqConnection
    {
        private readonly ILogger<RabbitMqConnection> _logger;
        private readonly IConnection? _connection;

        public RabbitMqConnection(RabbitMQOptions options, ILogger<RabbitMqConnection> logger)
        {
            _logger = logger;

            var factory = new ConnectionFactory()
            {
                HostName = options.Host,
                Port = int.Parse(options.Port),
                ClientProvidedName = options.ClientProvidedName,
                UserName = options.UserName,
                Password = options.Password,
            };

            try
            {
                _connection = factory.CreateConnection();
                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                _logger.LogInformation($"Connected to the message bus at {options.Host}:{options.Port}");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    $"Could not connect to the message bus at {options.Host}:{options.Port}: \n{ex.Message}");
            }
        }

        public IModel CreateChannel(ExchangeOptions options)
        {
            if (!IsOpen())
            {
                string message = "Can not create channel because connection is not open";

                _logger.LogError(message);
                throw new Exception(message);
            }

            IModel channel = _connection!.CreateModel();

            string exchangeType = options.IsFanout ? ExchangeType.Fanout : ExchangeType.Direct;

            channel.ExchangeDeclare(options.Exchange, exchangeType);

            if (!options.IsFanout)
            {
                channel.QueueDeclare(options.QueueName, false, false, false, null);
                channel.QueueBind(options.QueueName, options.Exchange, options.RoutingKey, null);
            }

            return channel;
        }

        public bool IsOpen()
        {
            return _connection != null && _connection.IsOpen;
        }

        public void Dispose()
        {
            _connection?.Close();
        }

        private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            _logger.LogInformation($"RabbitMQ connection shutdown");
        }
    }
}
