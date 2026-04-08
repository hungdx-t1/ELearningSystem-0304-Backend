FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .

RUN dotnet restore "ELearning.API/ELearning.API.csproj"
RUN dotnet publish "ELearning.API/ELearning.API.csproj" -c Release -o /app/publish

# Mượn máy chủ siêu nhẹ chỉ chứa Runtime để chạy App (Tiết kiệm RAM cho Render)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Mở cổng (Render sẽ tự động tiêm biến PORT vào đây)
EXPOSE 80
EXPOSE 443

# Lệnh khởi động app
ENTRYPOINT ["dotnet", "ELearning.API.dll"]