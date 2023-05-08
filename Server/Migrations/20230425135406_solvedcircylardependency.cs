using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class solvedcircylardependency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SessionMaps_Sessions_SessionId",
                table: "SessionMaps");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_SessionMapId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_SessionMaps_SessionId",
                table: "SessionMaps");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "SessionMaps");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SessionMapId",
                table: "Sessions",
                column: "SessionMapId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_SessionMapId",
                table: "Sessions");

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "SessionMaps",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SessionMapId",
                table: "Sessions",
                column: "SessionMapId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMaps_SessionId",
                table: "SessionMaps",
                column: "SessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionMaps_Sessions_SessionId",
                table: "SessionMaps",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id");
        }
    }
}
