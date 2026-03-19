namespace ELearning.Core.Entities;

public class Class
{
    public Guid Id { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;

    // Foreign key to Course
    public Guid CourseId { get; set; }
    public Course? Course { get; set; }

    public string? GoogleMeetLink { get; set; }
    public string? AcademicYear { get; set; }
    public string? Description { get; set; }

    // Navigation property
    public ICollection<ClassEnrollment> Enrollments { get; set; } = new List<ClassEnrollment>();
}