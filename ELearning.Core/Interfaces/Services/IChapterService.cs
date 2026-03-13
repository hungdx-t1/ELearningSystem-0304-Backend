using ELearning.Core.DTOs.Chapter;

namespace ELearning.Core.Interfaces.Services;

public interface IChapterService
{
    // Lấy tất cả các chương của một Khóa học
    Task<IEnumerable<ChapterResponseDto>> GetChaptersByCourseIdAsync(Guid courseId);
    
    Task<ChapterResponseDto?> GetChapterByIdAsync(Guid id);
    Task<ChapterResponseDto> CreateChapterAsync(CreateChapterRequestDto request);
    Task<bool> UpdateChapterAsync(Guid id, UpdateChapterRequestDto request);
    Task<bool> DeleteChapterAsync(Guid id);
}