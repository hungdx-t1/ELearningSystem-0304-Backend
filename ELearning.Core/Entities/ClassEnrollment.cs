namespace ELearning.Core.Entities;

public class ClassEnrollment
{
    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
}