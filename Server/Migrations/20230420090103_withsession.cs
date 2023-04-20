using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class withsession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HeroMapViews_Heroes_HeroId",
                table: "HeroMapViews");

            migrationBuilder.DropIndex(
                name: "IX_HeroMapViews_HeroId",
                table: "HeroMapViews");

            migrationBuilder.AddColumn<int>(
                name: "HeroMapId",
                table: "Planet",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HomePlanetId",
                table: "HeroMapViews",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "Argb",
                table: "Heroes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HeroMapId",
                table: "Heroes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "Heroes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "HeroMapId",
                table: "Edge",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Session",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionMapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Session_SessionMaps_SessionMapId",
                        column: x => x.SessionMapId,
                        principalTable: "SessionMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Planet_HeroMapId",
                table: "Planet",
                column: "HeroMapId");

            migrationBuilder.CreateIndex(
                name: "IX_HeroMapViews_HomePlanetId",
                table: "HeroMapViews",
                column: "HomePlanetId");

            migrationBuilder.CreateIndex(
                name: "IX_Heroes_HeroMapId",
                table: "Heroes",
                column: "HeroMapId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Heroes_SessionId",
                table: "Heroes",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Edge_HeroMapId",
                table: "Edge",
                column: "HeroMapId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_SessionMapId",
                table: "Session",
                column: "SessionMapId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Edge_HeroMapViews_HeroMapId",
                table: "Edge",
                column: "HeroMapId",
                principalTable: "HeroMapViews",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Heroes_HeroMapViews_HeroMapId",
                table: "Heroes",
                column: "HeroMapId",
                principalTable: "HeroMapViews",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Heroes_Session_SessionId",
                table: "Heroes",
                column: "SessionId",
                principalTable: "Session",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_HeroMapViews_Planet_HomePlanetId",
                table: "HeroMapViews",
                column: "HomePlanetId",
                principalTable: "Planet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Planet_HeroMapViews_HeroMapId",
                table: "Planet",
                column: "HeroMapId",
                principalTable: "HeroMapViews",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Edge_HeroMapViews_HeroMapId",
                table: "Edge");

            migrationBuilder.DropForeignKey(
                name: "FK_Heroes_HeroMapViews_HeroMapId",
                table: "Heroes");

            migrationBuilder.DropForeignKey(
                name: "FK_Heroes_Session_SessionId",
                table: "Heroes");

            migrationBuilder.DropForeignKey(
                name: "FK_HeroMapViews_Planet_HomePlanetId",
                table: "HeroMapViews");

            migrationBuilder.DropForeignKey(
                name: "FK_Planet_HeroMapViews_HeroMapId",
                table: "Planet");

            migrationBuilder.DropTable(
                name: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Planet_HeroMapId",
                table: "Planet");

            migrationBuilder.DropIndex(
                name: "IX_HeroMapViews_HomePlanetId",
                table: "HeroMapViews");

            migrationBuilder.DropIndex(
                name: "IX_Heroes_HeroMapId",
                table: "Heroes");

            migrationBuilder.DropIndex(
                name: "IX_Heroes_SessionId",
                table: "Heroes");

            migrationBuilder.DropIndex(
                name: "IX_Edge_HeroMapId",
                table: "Edge");

            migrationBuilder.DropColumn(
                name: "HeroMapId",
                table: "Planet");

            migrationBuilder.DropColumn(
                name: "HomePlanetId",
                table: "HeroMapViews");

            migrationBuilder.DropColumn(
                name: "Argb",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "HeroMapId",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "Heroes");

            migrationBuilder.DropColumn(
                name: "HeroMapId",
                table: "Edge");

            migrationBuilder.CreateIndex(
                name: "IX_HeroMapViews_HeroId",
                table: "HeroMapViews",
                column: "HeroId");

            migrationBuilder.AddForeignKey(
                name: "FK_HeroMapViews_Heroes_HeroId",
                table: "HeroMapViews",
                column: "HeroId",
                principalTable: "Heroes",
                principalColumn: "HeroId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
