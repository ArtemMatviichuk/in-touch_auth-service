namespace AuthService.Common.Dtos.MessageBusDtos
{
    public class RemovedUserDto : IMessageBusDto
    {
        public int Id { get; set; }
        public string Event => "User_Removed";
    }
}
