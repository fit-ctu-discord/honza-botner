using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HonzaBotner.Migrations
{
    public partial class StandUpStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StandUpStats",
                columns: table => new
                {
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Streak = table.Column<int>(type: "integer", nullable: false),
                    LongestStreak = table.Column<int>(type: "integer", nullable: false),
                    Freezes = table.Column<int>(type: "integer", nullable: false),
                    LastDayOfStreak = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastDayCompleted = table.Column<int>(type: "integer", nullable: false),
                    LastDayTasks = table.Column<int>(type: "integer", nullable: false),
                    TotalCompleted = table.Column<int>(type: "integer", nullable: false),
                    TotalTasks = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StandUpStats", x => x.UserId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StandUpStats");
        }
    }
}
