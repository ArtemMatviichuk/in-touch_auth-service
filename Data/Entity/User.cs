namespace AuthService.Data.Entity;
public class User {
    public int Id { get; set; }
    public string PublicId { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? PasswordHash { get; set; }

    public ICollection<UserRole>? Roles { get; set; }
    public EmailVerification? Verification { get; set; }

    public DateTime RegisteredDate { get; set; }
}