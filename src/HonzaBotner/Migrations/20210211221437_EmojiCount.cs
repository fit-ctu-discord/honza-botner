using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HonzaBotner.Migrations
{
    public partial class EmojiCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CountedEmojis",
                columns: table => new
                {
                    Id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Times = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    FirstUsedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountedEmojis", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Verifications_AuthId",
                table: "Verifications",
                column: "AuthId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CountedEmojis");

            migrationBuilder.DropIndex(
                name: "IX_Verifications_AuthId",
                table: "Verifications");
        }
    }
}
