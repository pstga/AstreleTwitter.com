using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstreleTwitter.com.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaToPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MediaPath",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaPath",
                table: "Posts");
        }
    }
}
