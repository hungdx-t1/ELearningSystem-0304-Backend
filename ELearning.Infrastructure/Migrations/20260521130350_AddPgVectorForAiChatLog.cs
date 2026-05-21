using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPgVectorForAiChatLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:lesson_type", "video,document,quiz,assignment")
                .Annotation("Npgsql:Enum:user_role", "admin,instructor,student")
                .Annotation("Npgsql:Enum:video_provider", "youtube,local_upload")
                .Annotation("Npgsql:PostgresExtension:vector", ",,")
                .OldAnnotation("Npgsql:Enum:lesson_type", "video,document,quiz,assignment")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,instructor,student")
                .OldAnnotation("Npgsql:Enum:video_provider", "youtube,local_upload");

            migrationBuilder.AddColumn<Vector>(
                name: "Embedding",
                table: "AiChatLogs",
                type: "vector",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Embedding",
                table: "AiChatLogs");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:lesson_type", "video,document,quiz,assignment")
                .Annotation("Npgsql:Enum:user_role", "admin,instructor,student")
                .Annotation("Npgsql:Enum:video_provider", "youtube,local_upload")
                .OldAnnotation("Npgsql:Enum:lesson_type", "video,document,quiz,assignment")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,instructor,student")
                .OldAnnotation("Npgsql:Enum:video_provider", "youtube,local_upload")
                .OldAnnotation("Npgsql:PostgresExtension:vector", ",,");
        }
    }
}
