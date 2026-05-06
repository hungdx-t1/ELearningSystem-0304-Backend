using System.ComponentModel.DataAnnotations;
using ELearning.Core.Common.Constants;

namespace ELearning.Core.DTOs.Auth;

public class ResetPasswordDto
{
    [Required]
    public string ResetToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [MinLength(8, ErrorMessage = "Mật khẩu phải từ 8 ký tự trở lên.")]
    [RegularExpression(ValidationConstants.PasswordRegexPattern, ErrorMessage = ValidationConstants.PasswordErrorMessage)]
    public string NewPassword { get; set; } = string.Empty;
}
