using ELearning.Core.Enums;

namespace ELearning.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string UserCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
    public string? AvatarUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? AdministrativeClass { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Security/OTP properties
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiryTime { get; set; }
    public string? ResetToken { get; set; }
    public string? PendingNewEmail { get; set; } // Temporary hold for email change

    // Navigation properties
    public ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();
    public ICollection<Class> InstructedClasses { get; set; } = new List<Class>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
}