namespace AuthService.Common.Dtos.MessageBusDtos;
public class PublishUserDto : IMessageBusDto
{
    public int Id { get; set; }
    public string Event => "User_Registered";
}