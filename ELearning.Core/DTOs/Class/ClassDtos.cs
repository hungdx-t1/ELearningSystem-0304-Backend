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
    string ClassCode, 
    string ClassName, 
    Guid InstructorId,
    string? GoogleMeetLink, 
    string? AcademicYear, 
    string? Description
);
public record EnrollStudentRequestDto(Guid StudentId);