using Microsoft.EntityFrameworkCore.Migrations;

namespace HonzaBotner.Migrations
{
    public partial class RenameReminderFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RemindAt",
                table: "Reminders",
                newName: "DateTime");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Reminders",
                newName: "Content");

            migrationBuilder.RenameIndex(
                name: "IX_Reminders_RemindAt",
                table: "Reminders",
                newName: "IX_Reminders_DateTime");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Reminders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "Reminders",
                newName: "RemindAt");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Reminders",
                newName: "Description");

            migrationBuilder.RenameIndex(
                name: "IX_Reminders_DateTime",
                table: "Reminders",
                newName: "IX_Reminders_RemindAt");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Reminders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
