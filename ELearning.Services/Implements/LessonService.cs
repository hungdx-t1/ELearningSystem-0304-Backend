using ELearning.Core.DTOs.Lesson;
using ELearning.Core.Entities;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;

namespace ELearning.Services.Implements;

public class LessonService : ILessonService
{
    private readonly IGenericRepository<Lesson> _lessonRepository;

    public LessonService(IGenericRepository<Lesson> lessonRepository)
    {
        _lessonRepository = lessonRepository;
    }

    public async Task<IEnumerable<LessonResponseDto>> GetLessonsByChapterIdAsync(Guid chapterId)
    {
        // Dùng hàm FindAsync của Generic Repository để lọc theo ChapterId
        var lessons = await _lessonRepository.FindAsync(l => l.ChapterId == chapterId);
        
        // Sắp xếp bài học theo thứ tự (SortOrder) để Frontend hiển thị cho đúng
        var sortedLessons = lessons.OrderBy(l => l.SortOrder);

        return sortedLessons.Select(l => new LessonResponseDto(
            l.Id, l.ChapterId, l.Title, l.Type, l.IsExam, l.VideoProvider, 
            l.VideoUrl, l.DocumentUrl, l.Duration, l.SortOrder
        ));
    }

    public async Task<LessonResponseDto?> GetLessonByIdAsync(Guid id)
    {
        var lesson = await _lessonRepository.GetByIdAsync(id);
        if (lesson == null) return null;

        return new LessonResponseDto(
            lesson.Id, lesson.ChapterId, lesson.Title, lesson.Type, lesson.IsExam, lesson.VideoProvider, 
            lesson.VideoUrl, lesson.DocumentUrl, lesson.Duration, lesson.SortOrder
        );
    }

    public async Task<LessonResponseDto> CreateLessonAsync(CreateLessonRequestDto request)
    {
        // 1. Tự động tính toán SortOrder nếu Frontend không truyền (hoặc truyền 0)
        int newSortOrder = request.SortOrder;
        if (newSortOrder <= 0)
        {
            var existingLessons = await _lessonRepository.FindAsync(l => l.ChapterId == request.ChapterId);
            newSortOrder = existingLessons.Any() ? existingLessons.Max(l => l.SortOrder) + 1 : 1;
        }

        // 2. Validate nghiệp vụ cơ bản (Bảo vệ dữ liệu rác)
        if (request.Type == LessonType.Video && string.IsNullOrEmpty(request.VideoUrl))
        {
            throw new ArgumentException("Bài học dạng Video bắt buộc phải có VideoUrl (Link Cloudinary hoặc Youtube).");
        }
        if (request.Type == LessonType.Document && string.IsNullOrEmpty(request.DocumentUrl))
        {
            throw new ArgumentException("Bài học dạng Tài liệu bắt buộc phải có DocumentUrl.");
        }

        // 3. Khởi tạo và Lưu DB
        var newLesson = new Lesson
        {
            Id = Guid.NewGuid(),
            ChapterId = request.ChapterId,
            Title = request.Title,
            Type = request.Type,
            IsExam = request.IsExam,
            VideoProvider = request.VideoProvider,
            VideoUrl = request.VideoUrl,
            DocumentUrl = request.DocumentUrl,
            Duration = request.Duration,
            SortOrder = newSortOrder // Đã dùng số tự tính
        };

        await _lessonRepository.AddAsync(newLesson);
        await _lessonRepository.SaveChangesAsync();

        return new LessonResponseDto(
            newLesson.Id, newLesson.ChapterId, newLesson.Title, newLesson.Type, newLesson.IsExam, newLesson.VideoProvider, 
            newLesson.VideoUrl, newLesson.DocumentUrl, newLesson.Duration, newLesson.SortOrder
        );
    }


    // TODO thêm LessonType
    public async Task<bool> UpdateLessonAsync(Guid id, UpdateLessonRequestDto request)
    {
        var lesson = await _lessonRepository.GetByIdAsync(id);
        if (lesson == null) return false;

        // Cập nhật thông tin
        lesson.Title = request.Title;
        lesson.Type = request.Type;
        lesson.IsExam = request.IsExam;
        lesson.VideoProvider = request.VideoProvider;
        lesson.VideoUrl = request.VideoUrl;
        lesson.DocumentUrl = request.DocumentUrl;
        lesson.Duration = request.Duration;
        lesson.SortOrder = request.SortOrder;

        _lessonRepository.Update(lesson);
        return await _lessonRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteLessonAsync(Guid id)
    {
        var lesson = await _lessonRepository.GetByIdAsync(id);
        if (lesson == null) return false;

        _lessonRepository.Delete(lesson);
        return await _lessonRepository.SaveChangesAsync();
    }
}