// {Root}/ELearning.API/Program.cs

using ELearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ELearning.Core.Interfaces;
using ELearning.Infrastructure.Repositories;
using ELearning.Core.Interfaces.Services;
using ELearning.Services.Implements;
using ELearning.Infrastructure.ExternalServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://e-learning-system-0304-frontend.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Cho phép gửi cookie từ frontend (nếu có)
    });
});

// Cấu hình Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHttpClient(); // Cho phép dùng HttpClient
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChapterService, ChapterService>();
builder.Services.AddScoped<ISubmissionService, SubmissionService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IAiChatService, AiChatService>();

builder.Services.AddScoped<IAiService, AiService>();

builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();

// Cấu hình Controllers và Swagger
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    // 1. Khai báo thẻ Bearer (Cất vào kho Components)
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        
        // Thêm ổ khóa vào kho với tên là "Bearer"
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Dán Token vào đây"
        });
        
        return Task.CompletedTask;
    });

    // 2. Móc ổ khóa lên tất cả các API
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        operation.Security ??= new List<OpenApiSecurityRequirement>();
        
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = new List<string>()
        });
        
        return Task.CompletedTask;
    });
});
var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors("AllowFrontend");

// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Đảm bảo Database luôn được cập nhật Migration mới nhất khi ứng dụng chạy
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        // Lệnh này tương đương với gõ 'Update-Database'
        context.Database.Migrate(); 
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Có lỗi xảy ra khi chạy Migration vào Database.");
    }
}

app.Run();