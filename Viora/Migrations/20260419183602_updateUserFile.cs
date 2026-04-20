using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Migrations
{
    /// <inheritdoc />
    public partial class updateUserFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "UserFiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TextLength",
                table: "UserFiles",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "UserFiles");

            migrationBuilder.DropColumn(
                name: "TextLength",
                table: "UserFiles");
        }
    }
}
