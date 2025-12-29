using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialPlatformTime.Migrations
{
    /// <inheritdoc />
    public partial class AddEditedDateForAComm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EditedDate",
                table: "Comments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditedDate",
                table: "Comments");
        }
    }
}
