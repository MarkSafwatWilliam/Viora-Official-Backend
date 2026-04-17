using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Migrations
{
    /// <inheritdoc />
    public partial class createRefreshTokenModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CityOfBirth",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MotherName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MotherName",
                table: "AspNetUsers");
        }
    }
}
