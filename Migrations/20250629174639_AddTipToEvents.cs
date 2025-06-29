using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zeiterfassung.server.Migrations
{
    /// <inheritdoc />
    public partial class AddTipToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "tip",
                table: "events",
                type: "decimal(65,30)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tip",
                table: "events");
        }
    }
}
