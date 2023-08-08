namespace AuthService.Common.Dtos;
public class TokenDto
{
    public TokenDto(string token, string type = "Bearer")
    {
        Type = type;
        Token = token;
    }

    public string Type { get; set; }
    public string Token { get; set; }
}