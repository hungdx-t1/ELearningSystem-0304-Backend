using System.ComponentModel.DataAnnotations;

namespace ELearning.Core.Common.Attributes;

public class AllowedEmailDomainAttribute(params string[] allowedDomains) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null) return ValidationResult.Success; // Nhường cho [Required] xử lý null

        var email = value.ToString();

        // Cắt lấy phần sau chữ @
        var domainPart = email?.Split('@').LastOrDefault();

        if (domainPart != null && allowedDomains.Any(domain => domain.Equals(domainPart, StringComparison.OrdinalIgnoreCase)))
        {
            return ValidationResult.Success;
        }

        var allowedListStr = string.Join(", ", allowedDomains);
        return new ValidationResult($"Email không hợp lệ. Hệ thống chỉ chấp nhận đuôi: {allowedListStr}");
    }
}
