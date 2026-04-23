using System.ComponentModel.DataAnnotations;

namespace ELearning.Core.DTOs.Auth;

public class ConfirmChangeEmailDto
{
    [Required]
    [EmailAddress]
    public string NewEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string OtpCode { get; set; } = string.Empty;
}
