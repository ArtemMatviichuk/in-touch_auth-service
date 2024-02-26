namespace AuthService.Common.Dtos.MessageBusDtos;
public class CreatedUserDto : IMessageBusDto
{
    public int Id { get; set; }
    public string PublicId { get; set; } = string.Empty;
    public string Event => "User_Registered";
}