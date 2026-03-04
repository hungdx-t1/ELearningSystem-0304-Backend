namespace ELearning.Core.Entities;

public class Submission
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    public string? SubmissionUrl { get; set; }
    public string? StudentNote { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public float? Score { get; set; }
    public string? Feedback { get; set; }
}