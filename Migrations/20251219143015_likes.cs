using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstreleTwitter.com.Migrations
{
    /// <inheritdoc />
    public partial class likes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LikeId",
                table: "Likes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Likes_LikeId",
                table: "Likes",
                column: "LikeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Likes_LikeId",
                table: "Likes",
                column: "LikeId",
                principalTable: "Likes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Likes_LikeId",
                table: "Likes");

            migrationBuilder.DropIndex(
                name: "IX_Likes_LikeId",
                table: "Likes");

            migrationBuilder.DropColumn(
                name: "LikeId",
                table: "Likes");
        }
    }
}
