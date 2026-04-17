using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Migrations
{
    /// <inheritdoc />
    public partial class updateAllChatModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EndMessageId",
                table: "ChatSummaries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StartMessageId",
                table: "ChatSummaries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LastSummary",
                table: "Chats",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSummarized",
                table: "ChatMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SenderType",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndMessageId",
                table: "ChatSummaries");

            migrationBuilder.DropColumn(
                name: "StartMessageId",
                table: "ChatSummaries");

            migrationBuilder.DropColumn(
                name: "LastSummary",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "IsSummarized",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "SenderType",
                table: "ChatMessages");
        }
    }
}
