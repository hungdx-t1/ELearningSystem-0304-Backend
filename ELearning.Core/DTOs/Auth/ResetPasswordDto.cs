using System.ComponentModel.DataAnnotations;

namespace ELearning.Core.DTOs.Auth;

public class ResetPasswordDto
{
    [Required]
    public string ResetToken { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
    public string NewPassword { get; set; } = string.Empty;
}
