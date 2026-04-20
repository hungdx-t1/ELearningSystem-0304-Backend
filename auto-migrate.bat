dotnet build
dotnet ef migrations add AddClassLessonSchedule --startup-project ../ELearning.API
dotnet ef database update --project ELearning.Infrastructure --startup-project ../ELearning.API

# Đứng ở thư mục backend/ gõ:
# dotnet ef migrations add AddClassLessonSchedule --project ELearning.Infrastructure --startup-project ELearning.API


dotnet ef database update --project ELearning.Infrastructure --startup-project ELearning.API
