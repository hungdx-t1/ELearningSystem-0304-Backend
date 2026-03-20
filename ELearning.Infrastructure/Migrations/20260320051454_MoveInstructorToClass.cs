using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ELearning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveInstructorToClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Users_InstructorId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_InstructorId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "InstructorId",
                table: "Courses");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "Classes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "InstructorId",
                table: "Classes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_CourseId",
                table: "Classes",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_InstructorId",
                table: "Classes",
                column: "InstructorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Courses_CourseId",
                table: "Classes",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Users_InstructorId",
                table: "Classes",
                column: "InstructorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Courses_CourseId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Users_InstructorId",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_CourseId",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_InstructorId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "InstructorId",
                table: "Classes");

            migrationBuilder.AddColumn<Guid>(
                name: "InstructorId",
                table: "Courses",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_InstructorId",
                table: "Courses",
                column: "InstructorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Users_InstructorId",
                table: "Courses",
                column: "InstructorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
