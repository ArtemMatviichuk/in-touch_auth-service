using AuthService.AppSettingsOptions;
using AuthService.AsyncDataServices.Interfaces;

namespace AuthService.AsyncDataServices.Implementations;
public class AuthMessageBusClient : MessageBusClient, IAuthMessageBusClient
{
    public AuthMessageBusClient(
        IRabbitMqConnection connection,
        RabbitMQOptions options,
        ILogger<EmailMessageBusClient> logger)
        : base(connection, options.Auth, logger)
    {
    }
}