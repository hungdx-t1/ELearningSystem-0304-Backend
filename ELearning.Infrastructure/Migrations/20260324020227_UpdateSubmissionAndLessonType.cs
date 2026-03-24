using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubmissionAndLessonType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Assignments_AssignmentId",
                table: "Submissions");

            migrationBuilder.RenameColumn(
                name: "AssignmentId",
                table: "Submissions",
                newName: "LessonId");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_AssignmentId",
                table: "Submissions",
                newName: "IX_Submissions_LessonId");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:lesson_type", "video,document,quiz,assignment")
                .Annotation("Npgsql:Enum:user_role", "admin,instructor,student")
                .Annotation("Npgsql:Enum:video_provider", "youtube,local_upload")
                .OldAnnotation("Npgsql:Enum:lesson_type", "video,document,quiz")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,instructor,student")
                .OldAnnotation("Npgsql:Enum:video_provider", "youtube,local_upload");

            migrationBuilder.AddColumn<Guid>(
                name: "ClassId",
                table: "Submissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_ClassId",
                table: "Submissions",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Classes_ClassId",
                table: "Submissions",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Lessons_LessonId",
                table: "Submissions",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Classes_ClassId",
                table: "Submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_Lessons_LessonId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_ClassId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "Submissions");

            migrationBuilder.RenameColumn(
                name: "LessonId",
                table: "Submissions",
                newName: "AssignmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Submissions_LessonId",
                table: "Submissions",
                newName: "IX_Submissions_AssignmentId");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:lesson_type", "video,document,quiz")
                .Annotation("Npgsql:Enum:user_role", "admin,instructor,student")
                .Annotation("Npgsql:Enum:video_provider", "youtube,local_upload")
                .OldAnnotation("Npgsql:Enum:lesson_type", "video,document,quiz,assignment")
                .OldAnnotation("Npgsql:Enum:user_role", "admin,instructor,student")
                .OldAnnotation("Npgsql:Enum:video_provider", "youtube,local_upload");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_Assignments_AssignmentId",
                table: "Submissions",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
