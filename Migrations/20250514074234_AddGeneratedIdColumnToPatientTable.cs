using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LABOGRA.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedIdColumnToPatientTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeneratedId",
                table: "Patients",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratedId",
                table: "Patients");
        }
    }
}
