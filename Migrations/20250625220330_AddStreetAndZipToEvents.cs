using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zeiterfassung.server.Migrations
{
    /// <inheritdoc />
    public partial class AddStreetAndZipToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "street",
                table: "events",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "zip",
                table: "events",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "street",
                table: "events");

            migrationBuilder.DropColumn(
                name: "zip",
                table: "events");
        }
    }
}
