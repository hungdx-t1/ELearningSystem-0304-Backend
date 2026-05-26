using ELearning.Core.DTOs.Class;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces.Services;
using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ELearning.Core.Interfaces;

namespace ELearning.Services.Implements;

public class ClassLessonScheduleService(IGenericRepository<ClassLessonSchedule> scheduleRepo) : IClassLessonScheduleService
{
    public async Task<ClassLessonScheduleResponseDto?> GetScheduleAsync(Guid classId, Guid lessonId)
    {
        var schedules = await scheduleRepo.FindAsync(s => s.ClassId == classId && s.LessonId == lessonId);
        var schedule = schedules.FirstOrDefault();

        if (schedule == null) return null;

        return new ClassLessonScheduleResponseDto(schedule.ClassId, schedule.LessonId, schedule.StartTime, schedule.DueDate, schedule.OverrideDuration);
    }

    public async Task<ClassLessonScheduleResponseDto> UpsertScheduleAsync(Guid classId, Guid lessonId, UpsertClassLessonScheduleRequestDto request)
    {
        var schedules = await scheduleRepo.FindAsync(s => s.ClassId == classId && s.LessonId == lessonId);
        var schedule = schedules.FirstOrDefault();

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
            await scheduleRepo.AddAsync(schedule);
        }
        else
        {
            schedule.StartTime = request.StartTime;
            schedule.DueDate = request.DueDate;
            schedule.OverrideDuration = request.OverrideDuration;
            scheduleRepo.Update(schedule);
        }

        await scheduleRepo.SaveChangesAsync();

        return new ClassLessonScheduleResponseDto(schedule.ClassId, schedule.LessonId, schedule.StartTime, schedule.DueDate, schedule.OverrideDuration);
    }
}
