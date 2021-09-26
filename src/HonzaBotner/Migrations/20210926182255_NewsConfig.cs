using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HonzaBotner.Migrations
{
    public partial class NewsConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    LastFetched = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ChannelsData = table.Column<string>(type: "text", nullable: false),
                    NewsProviderType = table.Column<string>(type: "text", nullable: false),
                    PublisherType = table.Column<string>(type: "text", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsConfigs_Name",
                table: "NewsConfigs",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsConfigs");
        }
    }
}
