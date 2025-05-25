using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LABOGRA.Migrations
{
    /// <inheritdoc />
    public partial class ReflectFinalModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "9af15b336e6a9619928537df30b2e6a2376569fcf9d7e773eccede65606529a0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "0000");
        }
    }
}
