using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AstreleTwitter.com.Migrations
{
    /// <inheritdoc />
    public partial class likecomm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MediaPath",
                table: "GroupMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "GroupComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GroupMessageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupComments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupComments_GroupMessages_GroupMessageId",
                        column: x => x.GroupMessageId,
                        principalTable: "GroupMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GroupMessageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupLikes_GroupMessages_GroupMessageId",
                        column: x => x.GroupMessageId,
                        principalTable: "GroupMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupComments_GroupMessageId",
                table: "GroupComments",
                column: "GroupMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupComments_UserId",
                table: "GroupComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupLikes_GroupMessageId",
                table: "GroupLikes",
                column: "GroupMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupLikes_UserId",
                table: "GroupLikes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupComments");

            migrationBuilder.DropTable(
                name: "GroupLikes");

            migrationBuilder.DropColumn(
                name: "MediaPath",
                table: "GroupMessages");
        }
    }
}
