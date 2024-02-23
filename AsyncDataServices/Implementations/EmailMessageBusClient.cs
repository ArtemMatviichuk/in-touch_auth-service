using AuthService.AppSettingsOptions;
using AuthService.AsyncDataServices.Interfaces;

namespace AuthService.AsyncDataServices.Implementations;
public class EmailMessageBusClient : MessageBusClient, IEmailMessageBusClient
{
    public EmailMessageBusClient(
        IRabbitMqConnection connection,
        RabbitMQOptions options,
        ILogger<EmailMessageBusClient> logger)
        : base(connection, options.Email, logger)
    {
    }
}