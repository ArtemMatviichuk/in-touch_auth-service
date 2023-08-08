namespace AuthService.Data.Entity;
public class User {
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }

    public ICollection<UserRole>? Roles { get; set; }

    public DateTime RegisteredDate { get; set; }
}