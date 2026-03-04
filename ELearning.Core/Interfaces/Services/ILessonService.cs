using ELearning.Core.DTOs.Lesson;

namespace ELearning.Core.Interfaces.Services;

public interface ILessonService
{
    // Hàm này rất quan trọng để hiển thị Menu bài học bên Frontend
    Task<IEnumerable<LessonResponseDto>> GetLessonsByChapterIdAsync(Guid chapterId);
    
    Task<LessonResponseDto?> GetLessonByIdAsync(Guid id);
    Task<LessonResponseDto> CreateLessonAsync(CreateLessonRequestDto request);
    Task<bool> UpdateLessonAsync(Guid id, UpdateLessonRequestDto request);
    Task<bool> DeleteLessonAsync(Guid id);
}