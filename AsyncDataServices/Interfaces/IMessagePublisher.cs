namespace AuthService.AsyncDataServices.Interfaces
{
    public interface IMessagePublisher
    {
        void PublishCreatedUser(int id, string publicId);
        void PublishRemovedUser(int id);
        void PublishEmailVerification(string email, string code, DateTime validTo);
    }
}
