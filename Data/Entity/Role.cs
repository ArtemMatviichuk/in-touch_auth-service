namespace AuthService.Data.Entity;
public class Role
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Value { get; set; }

    public IEnumerable<UserRole>? Users { get; set; }
}