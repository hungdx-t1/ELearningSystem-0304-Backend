namespace ELearning.Core.Entities;

public class ClassLessonSchedule
{
    // Khóa ngoại nối với Lớp Học
    public Guid ClassId { get; set; }
    public Class Class { get; set; } = null!;

    // Khóa ngoại nối với Bài thi/Bài tập
    public Guid LessonId { get; set; }
    public Lesson Lesson { get; set; } = null!;

    // Cấu hình linh hoạt cho từng Lớp
    public DateTime? StartTime { get; set; }
    public DateTime? DueDate { get; set; }
    public int? OverrideDuration { get; set; } // Ghi đè thời lượng (nếu lớp này cho 60p thay vì 45p mặc định)
}
