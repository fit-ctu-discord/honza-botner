using Microsoft.EntityFrameworkCore.Migrations;

namespace HonzaBotner.Migrations
{
    public partial class AddChannelIdToReminders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ChannelId",
                table: "Reminders",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChannelId",
                table: "Reminders");
        }
    }
}
