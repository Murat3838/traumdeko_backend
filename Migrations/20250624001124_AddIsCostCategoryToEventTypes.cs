using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace zeiterfassung.server.Migrations
{
    /// <inheritdoc />
    public partial class AddIsCostCategoryToEventTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_tblRole",
                table: "tblRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tblMitarbeiter",
                table: "tblMitarbeiter");

            migrationBuilder.RenameTable(
                name: "tblRole",
                newName: "tblrole");

            migrationBuilder.RenameTable(
                name: "tblMitarbeiter",
                newName: "tblmitarbeiter");

            migrationBuilder.AddColumn<bool>(
                name: "IsCostCategory",
                table: "event_types",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_tblrole",
                table: "tblrole",
                column: "username");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tblmitarbeiter",
                table: "tblmitarbeiter",
                column: "mit_username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_tblrole",
                table: "tblrole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_tblmitarbeiter",
                table: "tblmitarbeiter");

            migrationBuilder.DropColumn(
                name: "IsCostCategory",
                table: "event_types");

            migrationBuilder.RenameTable(
                name: "tblrole",
                newName: "tblRole");

            migrationBuilder.RenameTable(
                name: "tblmitarbeiter",
                newName: "tblMitarbeiter");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tblRole",
                table: "tblRole",
                column: "username");

            migrationBuilder.AddPrimaryKey(
                name: "PK_tblMitarbeiter",
                table: "tblMitarbeiter",
                column: "mit_username");
        }
    }
}
