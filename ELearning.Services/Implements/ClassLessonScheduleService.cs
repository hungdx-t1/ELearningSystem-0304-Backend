using ELearning.Core.DTOs.Class;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces.Services;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Services.Implements;

public class ClassLessonScheduleService : IClassLessonScheduleService
{
    private readonly AppDbContext _context;

    public ClassLessonScheduleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ClassLessonScheduleResponseDto?> GetScheduleAsync(Guid classId, Guid lessonId)
    {
        var schedule = await _context.ClassLessonSchedules
            .FirstOrDefaultAsync(s => s.ClassId == classId && s.LessonId == lessonId);

        if (schedule == null) return null;

        return new ClassLessonScheduleResponseDto(schedule.ClassId, schedule.LessonId, schedule.StartTime, schedule.DueDate, schedule.OverrideDuration);
    }

    public async Task<ClassLessonScheduleResponseDto> UpsertScheduleAsync(Guid classId, Guid lessonId, UpsertClassLessonScheduleRequestDto request)
    {
        var schedule = await _context.ClassLessonSchedules
            .FirstOrDefaultAsync(s => s.ClassId == classId && s.LessonId == lessonId);

        if (schedule == null)
        {
            schedule = new ClassLessonSchedule
            {
                ClassId = classId,
                LessonId = lessonId,
                StartTime = request.StartTime,
                DueDate = request.DueDate,
                OverrideDuration = request.OverrideDuration
            };
            _context.ClassLessonSchedules.Add(schedule);
        }
        else
        {
            schedule.StartTime = request.StartTime;
            schedule.DueDate = request.DueDate;
            schedule.OverrideDuration = request.OverrideDuration;
        }

        await _context.SaveChangesAsync();

        return new ClassLessonScheduleResponseDto(schedule.ClassId, schedule.LessonId, schedule.StartTime, schedule.DueDate, schedule.OverrideDuration);
    }
}
