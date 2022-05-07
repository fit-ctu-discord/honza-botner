using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
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
                    table.PrimaryKey("PK_StandUpStats", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StandUpStats");
        }
    }
}
