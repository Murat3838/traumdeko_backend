using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zeiterfassung.server.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalPayerToExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "original_payer",
                table: "expenses",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "original_payer",
                table: "expenses");
        }
    }
}
