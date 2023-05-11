using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class changedidstype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Edges_HeroMaps_HeroMapId",
                table: "Edges");

            migrationBuilder.DropForeignKey(
                name: "FK_Planets_HeroMaps_HeroMapId",
                table: "Planets");

            migrationBuilder.DropIndex(
                name: "IX_Planets_HeroMapId",
                table: "Planets");

            migrationBuilder.DropIndex(
                name: "IX_Heroes_HeroMapId",
                table: "Heroes");

            migrationBuilder.DropIndex(
                name: "IX_Edges_HeroMapId",
                table: "Edges");

            migrationBuilder.DropColumn(
                name: "HeroMapId",
                table: "Planets");

            migrationBuilder.DropColumn(
                name: "HeroMapId",
                table: "Edges");

            migrationBuilder.AddColumn<Guid>(
                name: "HeroMapViewId",
                table: "Planets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "LobbyInfos",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "HeroId",
                table: "HeroMaps",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "HeroMaps",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Heroes",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "HeroMapId",
                table: "Heroes",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "HeroId",
                table: "Heroes",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<Guid>(
                name: "HeroMapViewId",
                table: "Edges",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ApplicationUsers",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateIndex(
                name: "IX_Planets_HeroMapViewId",
                table: "Planets",
                column: "HeroMapViewId");

            migrationBuilder.CreateIndex(
                name: "IX_HeroMaps_HeroId",
                table: "HeroMaps",
                column: "HeroId");

            migrationBuilder.CreateIndex(
                name: "IX_Heroes_HeroMapId",
                table: "Heroes",
                column: "HeroMapId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_HeroMapViewId",
                table: "Edges",
                column: "HeroMapViewId");

            migrationBuilder.AddForeignKey(
                name: "FK_Edges_HeroMaps_HeroMapViewId",
                table: "Edges",
                column: "HeroMapViewId",
                principalTable: "HeroMaps",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HeroMaps_Heroes_HeroId",
                table: "HeroMaps",
                column: "HeroId",
                principalTable: "Heroes",
                principalColumn: "HeroId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Planets_HeroMaps_HeroMapViewId",
                table: "Planets",
                column: "HeroMapViewId",
                principalTable: "HeroMaps",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Edges_HeroMaps_HeroMapViewId",
                table: "Edges");

            migrationBuilder.DropForeignKey(
                name: "FK_HeroMaps_Heroes_HeroId",
                table: "HeroMaps");

            migrationBuilder.DropForeignKey(
                name: "FK_Planets_HeroMaps_HeroMapViewId",
                table: "Planets");

            migrationBuilder.DropIndex(
                name: "IX_Planets_HeroMapViewId",
                table: "Planets");

            migrationBuilder.DropIndex(
                name: "IX_HeroMaps_HeroId",
                table: "HeroMaps");

            migrationBuilder.DropIndex(
                name: "IX_Heroes_HeroMapId",
                table: "Heroes");

            migrationBuilder.DropIndex(
                name: "IX_Edges_HeroMapViewId",
                table: "Edges");

            migrationBuilder.DropColumn(
                name: "HeroMapViewId",
                table: "Planets");

            migrationBuilder.DropColumn(
                name: "HeroMapViewId",
                table: "Edges");

            migrationBuilder.AddColumn<int>(
                name: "HeroMapId",
                table: "Planets",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "LobbyInfos",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "HeroId",
                table: "HeroMaps",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "HeroMaps",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Heroes",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "HeroMapId",
                table: "Heroes",
                type: "int",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "HeroId",
                table: "Heroes",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "HeroMapId",
                table: "Edges",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ApplicationUsers",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.CreateIndex(
                name: "IX_Planets_HeroMapId",
                table: "Planets",
                column: "HeroMapId");

            migrationBuilder.CreateIndex(
                name: "IX_Heroes_HeroMapId",
                table: "Heroes",
                column: "HeroMapId",
                unique: true,
                filter: "[HeroMapId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_HeroMapId",
                table: "Edges",
                column: "HeroMapId");

            migrationBuilder.AddForeignKey(
                name: "FK_Edges_HeroMaps_HeroMapId",
                table: "Edges",
                column: "HeroMapId",
                principalTable: "HeroMaps",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Planets_HeroMaps_HeroMapId",
                table: "Planets",
                column: "HeroMapId",
                principalTable: "HeroMaps",
                principalColumn: "Id");
        }
    }
}
