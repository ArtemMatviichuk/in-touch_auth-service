namespace AuthService.AsyncDataServices.Interfaces
{
    public interface IMessagePublisher
    {
        void PublishCreatedUser(int id);
        void PublishRemovedUser(int id);
        void PublishEmailVerification(string email, string code, DateTime validTo);
    }
}
