dotnet build
dotnet ef migrations add MoveInstructorToClass --startup-project ../ELearning.API
dotnet ef database update --startup-project ../ELearning.API

# Đứng ở thư mục backend/ gõ:
# dotnet ef migrations add MoveInstructorToClass --project ELearning.Infrastructure --startup-project ELearning.API