using System.ComponentModel.DataAnnotations;

namespace Backend.Communication.Incoming;

public class LoginData
{
    [Required]
    [Length(1, 200, ErrorMessage = "Username cannot be empty or over 200 characters!")]
    public string UserName { get; set; } = null!;

    [Required]
    [Length(6, 100, ErrorMessage = "Password has to be between 10-100 characters!")]
    public string Password { get; set; } = null!;
}
