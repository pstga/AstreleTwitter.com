using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstreleTwitter.com.Migrations
{
    /// <inheritdoc />
    public partial class grup2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessage_AspNetUsers_UserId",
                table: "GroupMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessage_Groups_GroupId",
                table: "GroupMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupMessage",
                table: "GroupMessage");

            migrationBuilder.RenameTable(
                name: "GroupMessage",
                newName: "GroupMessages");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMessage_UserId",
                table: "GroupMessages",
                newName: "IX_GroupMessages_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMessage_GroupId",
                table: "GroupMessages",
                newName: "IX_GroupMessages_GroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupMessages",
                table: "GroupMessages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessages_Groups_GroupId",
                table: "GroupMessages",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_AspNetUsers_UserId",
                table: "GroupMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_GroupMessages_Groups_GroupId",
                table: "GroupMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GroupMessages",
                table: "GroupMessages");

            migrationBuilder.RenameTable(
                name: "GroupMessages",
                newName: "GroupMessage");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMessages_UserId",
                table: "GroupMessage",
                newName: "IX_GroupMessage_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupMessages_GroupId",
                table: "GroupMessage",
                newName: "IX_GroupMessage_GroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GroupMessage",
                table: "GroupMessage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessage_AspNetUsers_UserId",
                table: "GroupMessage",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GroupMessage_Groups_GroupId",
                table: "GroupMessage",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
