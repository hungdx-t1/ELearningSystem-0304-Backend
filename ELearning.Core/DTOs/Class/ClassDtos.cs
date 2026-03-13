namespace ELearning.Core.DTOs.Class;

public record ClassResponseDto(Guid Id, string ClassCode, string ClassName, string? GoogleMeetLink, string? AcademicYear, string? Description);
public record CreateClassRequestDto(string ClassCode, string ClassName, string? GoogleMeetLink, string? AcademicYear, string? Description);
public record EnrollStudentRequestDto(Guid StudentId);