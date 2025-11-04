using System.ComponentModel.DataAnnotations;

namespace Backend.Communication.Incoming;

public class ResetPasswordData : LoginData
{
    [Required]
    [Length(36, 36, ErrorMessage = "The key is not the correct length!")]
    public string SecretKey { get; set; } = null!;
}
