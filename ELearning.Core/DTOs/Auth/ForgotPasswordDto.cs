using System.ComponentModel.DataAnnotations;
using ELearning.Core.Common.Attributes;

namespace ELearning.Core.DTOs.Auth;

public class ForgotPasswordDto
{
    [Required(ErrorMessage = "Vui lòng nhập Email.")]
    [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
    [AllowedEmailDomain("gmail.com", "outlook.com", "outlook.com.vn")]
    public string Email { get; set; } = string.Empty;
}