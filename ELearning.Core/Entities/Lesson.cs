namespace ELearning.Core.Entities;

using ELearning.Core.Enums;

public class Lesson
{
    public Guid Id { get; set; }
    public Guid ChapterId { get; set; }
    public Chapter Chapter { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public LessonType Type { get; set; } = LessonType.Video;
    public VideoProvider? VideoProvider { get; set; }
    public string? VideoUrl { get; set; }
    public string? DocumentUrl { get; set; }
    public int? Duration { get; set; }
    public int SortOrder { get; set; } = 0;

    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}