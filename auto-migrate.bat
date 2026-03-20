dotnet build
dotnet ef migrations add MoveInstructorToClass --startup-project ../ELearning.API
dotnet ef database update --startup-project ../ELearning.API