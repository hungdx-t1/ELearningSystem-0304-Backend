namespace ELearning.Core.Enums;

public enum UserRole
{
    Admin,
    Instructor,
    Student
}

public enum LessonType
{
    Video = 0,
    Document = 1,
    Quiz = 2,
    Assignment = 3
}

public enum VideoProvider
{
    Youtube,
    LocalUpload
}