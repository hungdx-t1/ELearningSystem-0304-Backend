using System.ComponentModel.DataAnnotations;

namespace ELearning.Core.DTOs.Auth;

public class RequestChangeEmailDto
{
    [Required]
    [EmailAddress]
    public string NewEmail { get; set; } = string.Empty;
}
