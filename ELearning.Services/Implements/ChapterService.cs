using ELearning.Core.DTOs.Chapter;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;

namespace ELearning.Services.Implements;

public class ChapterService : IChapterService
{
    private readonly IGenericRepository<Chapter> _chapterRepository;

    public ChapterService(IGenericRepository<Chapter> chapterRepository)
    {
        _chapterRepository = chapterRepository;
    }

    public async Task<IEnumerable<ChapterResponseDto>> GetChaptersByCourseIdAsync(Guid courseId)
    {
        var chapters = await _chapterRepository.FindAsync(c => c.CourseId == courseId);
        
        // Sắp xếp theo thứ tự hiển thị
        var sortedChapters = chapters.OrderBy(c => c.SortOrder);

        return sortedChapters.Select(c => new ChapterResponseDto(c.Id, c.CourseId, c.Title, c.SortOrder));
    }

    public async Task<ChapterResponseDto?> GetChapterByIdAsync(Guid id)
    {
        var chapter = await _chapterRepository.GetByIdAsync(id);
        if (chapter == null) return null;

        return new ChapterResponseDto(chapter.Id, chapter.CourseId, chapter.Title, chapter.SortOrder);
    }

    public async Task<ChapterResponseDto> CreateChapterAsync(CreateChapterRequestDto request)
    {
        // Tự động tính SortOrder nếu không truyền
        int newSortOrder = request.SortOrder;
        if (newSortOrder <= 0)
        {
            var existingChapters = await _chapterRepository.FindAsync(c => c.CourseId == request.CourseId);
            newSortOrder = existingChapters.Any() ? existingChapters.Max(c => c.SortOrder) + 1 : 1;
        }

        var newChapter = new Chapter
        {
            Id = Guid.NewGuid(),
            CourseId = request.CourseId,
            Title = request.Title,
            SortOrder = newSortOrder
        };

        await _chapterRepository.AddAsync(newChapter);
        await _chapterRepository.SaveChangesAsync();

        return new ChapterResponseDto(newChapter.Id, newChapter.CourseId, newChapter.Title, newChapter.SortOrder);
    }

    public async Task<bool> UpdateChapterAsync(Guid id, UpdateChapterRequestDto request)
    {
        var chapter = await _chapterRepository.GetByIdAsync(id);
        if (chapter == null) return false;

        chapter.Title = request.Title;
        chapter.SortOrder = request.SortOrder;

        _chapterRepository.Update(chapter);
        return await _chapterRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteChapterAsync(Guid id)
    {
        var chapter = await _chapterRepository.GetByIdAsync(id);
        if (chapter == null) return false;

        _chapterRepository.Delete(chapter);
        return await _chapterRepository.SaveChangesAsync();
    }
}