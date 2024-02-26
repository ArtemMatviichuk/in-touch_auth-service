using AuthService.AsyncDataServices.Interfaces;
using AuthService.Common.Dtos.MessageBusDtos;

namespace AuthService.AsyncDataServices.Implementations
{
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IAuthMessageBusClient _authBusClient;
        private readonly IEmailMessageBusClient _emailBusClient;
        private readonly ILogger<MessagePublisher> _logger;

        public MessagePublisher(IAuthMessageBusClient authBusClient, IEmailMessageBusClient emailBusClient,
            ILogger<MessagePublisher> logger)
        {
            _authBusClient = authBusClient;
            _emailBusClient = emailBusClient;
            _logger = logger;
        }

        public void PublishCreatedUser(int id, string publicId)
        {
            try
            {
                _authBusClient.SendMessage(new CreatedUserDto() { Id = id, PublicId = publicId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not send RabbitMQ message: \n{ex.Message}");
            }
        }

        public void PublishRemovedUser(int id)
        {
            try
            {
                _authBusClient.SendMessage(new RemovedUserDto() { Id = id });
                _emailBusClient.SendMessage(new RemovedUserDto() { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not send RabbitMQ message: \n{ex.Message}");
            }
        }

        public void PublishEmailVerification(string email, string code, DateTime validTo)
        {
            try
            {
                _emailBusClient.SendMessage(new VerifyEmailDto() { Email = email, VerificationCode = code, ValidTo = validTo });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Could not send RabbitMQ message: \n{ex.Message}");
            }
        }
    }
}
