using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zeiterfassung.server.Migrations
{
    /// <inheritdoc />
    public partial class AddDecorationAndCateringToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Backdrop",
                table: "events",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Catering",
                table: "events",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CateringCount",
                table: "events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dish",
                table: "events",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "GuestCount",
                table: "events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GuestTables",
                table: "events",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Backdrop",
                table: "events");

            migrationBuilder.DropColumn(
                name: "Catering",
                table: "events");

            migrationBuilder.DropColumn(
                name: "CateringCount",
                table: "events");

            migrationBuilder.DropColumn(
                name: "Dish",
                table: "events");

            migrationBuilder.DropColumn(
                name: "GuestCount",
                table: "events");

            migrationBuilder.DropColumn(
                name: "GuestTables",
                table: "events");
        }
    }
}
