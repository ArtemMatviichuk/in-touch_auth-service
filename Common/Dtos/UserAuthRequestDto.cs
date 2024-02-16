using System.ComponentModel.DataAnnotations;

namespace AuthService.Common.Dtos;
public class UserAuthRequestDto
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string? Email { get; set; }

    [Required]
    [MinLength(8)]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*[!@#$&*])(?=.*[0-9])(?=.*[a-z]).{8,40}$",
        ErrorMessage = @"Not allowed password. Criteria:
8-40 characters length
1 letters in upper case
1 special character (!@#$&*)
1 numerals (0-9)
1 letters in lower case")]
    public string? Password { get; set; }
}