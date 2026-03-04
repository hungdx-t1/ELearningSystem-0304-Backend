namespace ELearning.Core.Entities;

public class Course
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public Guid? InstructorId { get; set; }
    public User? Instructor { get; set; }

    public string? ThumbnailUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
}