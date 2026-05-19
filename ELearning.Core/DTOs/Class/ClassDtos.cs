namespace ELearning.Core.DTOs.Class;

public record ClassResponseDto(
    Guid Id,
    Guid CourseId,
    string ClassCode,
    string ClassName,
    Guid InstructorId,
    string? GoogleMeetLink,
    string? AcademicYear,
    string? Description
);

public record CreateClassRequestDto(
    Guid CourseId,
    string ClassCode,
    string ClassName,
    Guid InstructorId,
    string? GoogleMeetLink,
    string? AcademicYear,
    string? Description
);
public record UpdateClassRequestDto(
    Guid CourseId,
    string ClassCode,
    string ClassName,
    Guid InstructorId,
    string? GoogleMeetLink,
    string? AcademicYear,
    string? Description
);
public record EnrollStudentRequestDto(Guid StudentId);

public record EnrollStudentByEmailRequestDto(string EmailOrCode);

public record ClassStudentDto(
    Guid Id,
    string FullName,
    string Email,
    string StudentCode,
    string JoinDate
);

public record ClassDetailsResponseDto(
    Guid Id,
    string ClassCode,
    string ClassName,
    string CourseName,
    string? GoogleMeetLink,
    string? AcademicYear,
    List<ClassStudentDto> Students
);

public record StudentClassResponseDto(
    Guid Id,
    Guid CourseId,
    string ClassCode,
    string ClassName,
    string CourseName,
    string? Schedule,
    string? GoogleMeetLink
);