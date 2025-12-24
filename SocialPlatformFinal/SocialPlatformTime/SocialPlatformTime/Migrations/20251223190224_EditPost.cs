using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialPlatformTime.Migrations
{
    /// <inheritdoc />
    public partial class EditPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VideoURL",
                table: "Posts",
                newName: "Video");

            migrationBuilder.RenameColumn(
                name: "ImageURL",
                table: "Posts",
                newName: "Image");

            migrationBuilder.AlterColumn<string>(
                name: "PostDescription",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Video",
                table: "Posts",
                newName: "VideoURL");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Posts",
                newName: "ImageURL");

            migrationBuilder.AlterColumn<string>(
                name: "PostDescription",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
