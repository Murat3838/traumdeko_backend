using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zeiterfassung.server.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypeAndFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "event_type_id",
                table: "events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "events",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "total_amount",
                table: "events",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "event_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_event_types", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_events_event_type_id",
                table: "events",
                column: "event_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_events_event_types_event_type_id",
                table: "events",
                column: "event_type_id",
                principalTable: "event_types",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_events_event_types_event_type_id",
                table: "events");

            migrationBuilder.DropTable(
                name: "event_types");

            migrationBuilder.DropIndex(
                name: "IX_events_event_type_id",
                table: "events");

            migrationBuilder.DropColumn(
                name: "event_type_id",
                table: "events");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "events");

            migrationBuilder.DropColumn(
                name: "total_amount",
                table: "events");
        }
    }
}
