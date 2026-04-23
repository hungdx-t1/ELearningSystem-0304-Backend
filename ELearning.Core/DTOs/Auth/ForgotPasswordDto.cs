using System.ComponentModel.DataAnnotations;

namespace ELearning.Core.DTOs.Auth;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
