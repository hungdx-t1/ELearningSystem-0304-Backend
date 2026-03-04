// {Root}/ELearning.API/Program.cs

using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ELearning.Core.Interfaces;
using ELearning.Infrastructure.Repositories;
using ELearning.Core.Interfaces.Services;
using ELearning.Services.Implements;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICourseService, CourseService>();

// 2. Cấu hình Controllers và Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Đảm bảo đã cài Swashbuckle.AspNetCore

var app = builder.Build();

// 3. Pipeline xử lý request
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();