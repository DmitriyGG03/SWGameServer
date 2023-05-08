using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class changedsessionmap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SessionMaps_Heroes_HeroId",
                table: "SessionMaps");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_SessionMapId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_SessionMaps_HeroId",
                table: "SessionMaps");

            migrationBuilder.DropColumn(
                name: "HeroId",
                table: "SessionMaps");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "HeroId",
                table: "SessionMaps",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SessionMapId",
                table: "Sessions",
                column: "SessionMapId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SessionMaps_HeroId",
                table: "SessionMaps",
                column: "HeroId");

            migrationBuilder.AddForeignKey(
                name: "FK_SessionMaps_Heroes_HeroId",
                table: "SessionMaps",
                column: "HeroId",
                principalTable: "Heroes",
                principalColumn: "HeroId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
