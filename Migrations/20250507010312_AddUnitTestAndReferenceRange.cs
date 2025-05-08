using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LABOGRA.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitTestAndReferenceRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferenceRange",
                table: "Tests",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Tests",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferenceRange",
                table: "Tests");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Tests");
        }
    }
}
