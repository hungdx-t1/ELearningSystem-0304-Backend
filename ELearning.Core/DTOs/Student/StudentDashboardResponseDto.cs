using ELearning.Core.DTOs.Course;
using ELearning.Core.DTOs.Class;

namespace ELearning.Core.DTOs.Student;

public record StudentClassDto(
    Guid Id,
    Guid CourseId,
    string ClassCode,
    string ClassName,
    string CourseName,
    string? Schedule,
    string? GoogleMeetLink
);

public record StudentDashboardResponseDto(
    List<CourseResponseDto> AllCourses,
    List<StudentClassDto> MyClasses,
    int CompletedCount,
    float AverageScore
);
