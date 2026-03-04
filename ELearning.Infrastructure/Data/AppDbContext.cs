using ELearning.Core.Entities;
using ELearning.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<ClassEnrollment> ClassEnrollments => Set<ClassEnrollment>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<AiChatLog> AiChatLogs => Set<AiChatLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Ánh xạ Enum vào PostgreSQL
        modelBuilder.HasPostgresEnum<UserRole>();
        modelBuilder.HasPostgresEnum<LessonType>();
        modelBuilder.HasPostgresEnum<VideoProvider>();

        // 2. Cấu hình bảng Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.UserCode).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // 3. Cấu hình khóa chính phức hợp (Composite Key) cho ClassEnrollment
        modelBuilder.Entity<ClassEnrollment>()
            .HasKey(ce => new { ce.ClassId, ce.StudentId });

        modelBuilder.Entity<ClassEnrollment>()
            .HasOne(ce => ce.Class)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(ce => ce.ClassId);

        modelBuilder.Entity<ClassEnrollment>()
            .HasOne(ce => ce.Student)
            .WithMany(u => u.ClassEnrollments)
            .HasForeignKey(ce => ce.StudentId);

        // 4. Cấu hình các quan hệ (Relation) khác có hành vi Delete Behavior
        modelBuilder.Entity<Course>()
            .HasOne(c => c.Instructor)
            .WithMany(u => u.InstructedCourses)
            .HasForeignKey(c => c.InstructorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Student)
            .WithMany(u => u.Submissions)
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}