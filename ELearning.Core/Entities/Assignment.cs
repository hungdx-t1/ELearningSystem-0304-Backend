namespace ELearning.Core.Entities;

public class Assignment
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public float MaxScore { get; set; } = 10.0f;

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}