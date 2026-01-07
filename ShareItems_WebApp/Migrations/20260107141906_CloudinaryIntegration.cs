using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShareItems_WebApp.Migrations
{
    /// <inheritdoc />
    public partial class CloudinaryIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "NoteFiles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "NoteFiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            // Make FilePath nullable for Cloudinary integration
            migrationBuilder.AlterColumn<string>(
                name: "FilePath",
                table: "NoteFiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "NoteFiles");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "NoteFiles");
        }
    }
}
