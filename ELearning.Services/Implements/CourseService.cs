using ELearning.Core.DTOs.Course;
using ELearning.Core.Entities;
using ELearning.Core.Enums;
using ELearning.Core.Interfaces;
using ELearning.Core.Interfaces.Services;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Services.Implements;

public class CourseService(IGenericRepository<Course> courseRepository, AppDbContext context) : ICourseService
{
    public async Task<IEnumerable<CourseResponseDto>> GetAllCoursesAsync()
    {
        var courses = await context.Courses.Include(c => c.Creator).ToListAsync();
        return courses.Select(c => new CourseResponseDto(
            c.Id, c.Title, c.Description, c.ThumbnailUrl, c.CreatedAt, c.CreatorId, c.Creator?.FullName, c.IsPublic
        ));
    }

    public async Task<CourseResponseDto?> GetCourseByIdAsync(Guid id)
    {
        var course = await context.Courses.Include(c => c.Creator).FirstOrDefaultAsync(c => c.Id == id);
        if (course == null) return null;

        return new CourseResponseDto(
            course.Id, course.Title, course.Description, course.ThumbnailUrl, course.CreatedAt, course.CreatorId, course.Creator?.FullName, course.IsPublic
        );
    }

    public async Task<CourseResponseDto> CreateCourseAsync(CreateCourseRequestDto request, Guid creatorId)
    {
        var newCourse = new Course
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            CreatedAt = DateTime.UtcNow,
            CreatorId = creatorId,
            IsPublic = request.IsPublic // Lấy trạng thái công khai
        };

        await courseRepository.AddAsync(newCourse);
        await courseRepository.SaveChangesAsync();

        return new CourseResponseDto(newCourse.Id, newCourse.Title, newCourse.Description, newCourse.ThumbnailUrl, newCourse.CreatedAt, newCourse.CreatorId, null, newCourse.IsPublic);
    }

    public async Task<bool> UpdateCourseAsync(Guid id, UpdateCourseRequestDto request)
    {
        var course = await courseRepository.GetByIdAsync(id);
        if (course == null) return false;

        course.Title = request.Title;
        course.Description = request.Description;
        course.ThumbnailUrl = request.ThumbnailUrl;
        course.IsPublic = request.IsPublic; // Cập nhật trạng thái

        courseRepository.Update(course);
        return await courseRepository.SaveChangesAsync();
    }

    public async Task<bool> DeleteCourseAsync(Guid id)
    {
        var course = await courseRepository.GetByIdAsync(id);
        if (course == null) return false;

        courseRepository.Delete(course);
        return await courseRepository.SaveChangesAsync();
    }

    // 🌟 THUẬT TOÁN DEEP COPY: NHÂN BẢN KHÓA HỌC
    public async Task<CourseResponseDto?> CopyCourseAsync(Guid courseId, Guid newCreatorId)
    {
        // 1. Kéo toàn bộ Khóa học -> Chương -> Bài học lên bằng AsNoTracking (Rất quan trọng để EF không bị nhầm lẫn ID)
        var originalCourse = await context.Courses
            .Include(c => c.Chapters)
                .ThenInclude(ch => ch.Lessons)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == courseId);

        // Chỉ cho copy nếu khóa học tồn tại và đang được Public
        if (originalCourse == null || !originalCourse.IsPublic) return null;

        // 2. Tạo Khóa học mới
        var newCourse = new Course
        {
            Id = Guid.NewGuid(),
            Title = originalCourse.Title + " (Bản sao)", // Đánh dấu là bản copy
            Description = originalCourse.Description,
            ThumbnailUrl = originalCourse.ThumbnailUrl,
            CreatedAt = DateTime.UtcNow,
            CreatorId = newCreatorId, // Đổi chủ
            IsPublic = false, // Bản copy mặc định là Riêng tư
            Chapters = []
        };

        // 3. Quét qua từng Chương để tạo mới
        foreach (var originalChapter in originalCourse.Chapters)
        {
            var newChapter = new Chapter
            {
                Id = Guid.NewGuid(),
                Title = originalChapter.Title,
                SortOrder = originalChapter.SortOrder,
                CourseId = newCourse.Id,
                Lessons = []
            };

            // 4. Quét qua từng Bài học trong Chương để tạo mới
            foreach (var originalLesson in originalChapter.Lessons)
            {
                var newLesson = new Lesson
                {
                    Id = Guid.NewGuid(),
                    Title = originalLesson.Title,
                    Type = originalLesson.Type,
                    VideoProvider = originalLesson.VideoProvider, // Copy Provider (Youtube/Cloudinary...)
                    VideoUrl = originalLesson.VideoUrl,           // Copy link Video
                    DocumentUrl = originalLesson.DocumentUrl,     // Copy link Tài liệu
                    SortOrder = originalLesson.SortOrder,
                    Duration = originalLesson.Duration,
                    ChapterId = newChapter.Id
                };
                newChapter.Lessons.Add(newLesson);
            }
            newCourse.Chapters.Add(newChapter);
        }

        // Lưu toàn bộ khối dữ liệu khổng lồ này xuống Database 1 lần duy nhất
        await context.Courses.AddAsync(newCourse);
        await context.SaveChangesAsync();

        return new CourseResponseDto(newCourse.Id, newCourse.Title, newCourse.Description, newCourse.ThumbnailUrl, newCourse.CreatedAt, newCourse.CreatorId, null, newCourse.IsPublic);
    }

    public async Task<IEnumerable<AssignmentDto>> GetAssignmentsByCourseAsync(Guid courseId)
    {
        return await context.Lessons
            .Include(l => l.Chapter)
            .Where(l => l.Chapter.CourseId == courseId && l.Type == LessonType.Assignment)
            .Select(l => new AssignmentDto(l.Id, l.Title, l.Chapter.Title))
            .ToListAsync();
    }
}