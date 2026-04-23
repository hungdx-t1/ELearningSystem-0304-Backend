using System.ComponentModel.DataAnnotations;

namespace ELearning.Core.DTOs.Auth;

public class VerifyOtpDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string OtpCode { get; set; } = string.Empty;
}
