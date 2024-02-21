namespace AuthService.Data.Entity
{
    public class EmailVerification
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DateTime ValidTo { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
