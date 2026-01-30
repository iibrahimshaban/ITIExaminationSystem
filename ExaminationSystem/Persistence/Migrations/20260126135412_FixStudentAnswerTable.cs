using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExaminationSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixStudentAnswerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnsweredAt",
                table: "StudentAnswers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AnsweredAt",
                table: "StudentAnswers",
                type: "datetime2",
                nullable: true);
        }
    }
}
