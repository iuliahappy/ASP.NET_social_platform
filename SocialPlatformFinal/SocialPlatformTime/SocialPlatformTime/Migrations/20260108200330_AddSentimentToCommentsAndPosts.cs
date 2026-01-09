using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialPlatformTime.Migrations
{
    /// <inheritdoc />
    public partial class AddSentimentToCommentsAndPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SentimentAnalyzedAt",
                table: "Posts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SentimentConfidence",
                table: "Posts",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SentimentLabel",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentimentAnalyzedAt",
                table: "Comments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SentimentConfidence",
                table: "Comments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SentimentLabel",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentimentAnalyzedAt",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "SentimentConfidence",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "SentimentLabel",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "SentimentAnalyzedAt",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "SentimentConfidence",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "SentimentLabel",
                table: "Comments");
        }
    }
}
