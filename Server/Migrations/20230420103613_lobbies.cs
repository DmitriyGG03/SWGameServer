using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class lobbies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Salt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lobbies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LobbyName = table.Column<string>(type: "nvarchar(256)", nullable: false),
                    MaxHeroNumbers = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lobbies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Points",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    X = table.Column<float>(type: "real", nullable: false),
                    Y = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Points", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LobbyInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Argb = table.Column<int>(type: "int", nullable: false),
                    Ready = table.Column<bool>(type: "bit", nullable: false),
                    LobbyLeader = table.Column<bool>(type: "bit", nullable: false),
                    LobbyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LobbyInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LobbyInfos_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LobbyInfos_Lobbies_LobbyId",
                        column: x => x.LobbyId,
                        principalTable: "Lobbies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Edges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromPlanetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToPlanetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeroMapId = table.Column<int>(type: "int", nullable: true),
                    SessionMapId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Edges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Heroes",
                columns: table => new
                {
                    HeroId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resourses = table.Column<int>(type: "int", nullable: false),
                    ResearchShipLimit = table.Column<byte>(type: "tinyint", nullable: false),
                    ColonizationShipLimit = table.Column<byte>(type: "tinyint", nullable: false),
                    Argb = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    HeroMapId = table.Column<int>(type: "int", nullable: true),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Heroes", x => x.HeroId);
                    table.ForeignKey(
                        name: "FK_Heroes_ApplicationUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionMaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeroId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionMaps_Heroes_HeroId",
                        column: x => x.HeroId,
                        principalTable: "Heroes",
                        principalColumn: "HeroId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionMapId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_SessionMaps_SessionMapId",
                        column: x => x.SessionMapId,
                        principalTable: "SessionMaps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HeroMaps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HeroId = table.Column<int>(type: "int", nullable: false),
                    HomePlanetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HeroMaps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Planets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HeroMapId = table.Column<int>(type: "int", nullable: true),
                    SessionMapId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Planets_HeroMaps_HeroMapId",
                        column: x => x.HeroMapId,
                        principalTable: "HeroMaps",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Planets_Points_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Points",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Planets_SessionMaps_SessionMapId",
                        column: x => x.SessionMapId,
                        principalTable: "SessionMaps",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Edges_HeroMapId",
                table: "Edges",
                column: "HeroMapId");

            migrationBuilder.CreateIndex(
                name: "IX_Edges_SessionMapId",
                table: "Edges",
                column: "SessionMapId");

            migrationBuilder.CreateIndex(
                name: "IX_Heroes_HeroMapId",
                table: "Heroes",
                column: "HeroMapId",
                unique: true,
                filter: "[HeroMapId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Heroes_SessionId",
                table: "Heroes",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Heroes_UserId",
                table: "Heroes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HeroMaps_HomePlanetId",
                table: "HeroMaps",
                column: "HomePlanetId");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyInfos_LobbyId",
                table: "LobbyInfos",
                column: "LobbyId");

            migrationBuilder.CreateIndex(
                name: "IX_LobbyInfos_UserId",
                table: "LobbyInfos",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Planets_HeroMapId",
                table: "Planets",
                column: "HeroMapId");

            migrationBuilder.CreateIndex(
                name: "IX_Planets_PositionId",
                table: "Planets",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_Planets_SessionMapId",
                table: "Planets",
                column: "SessionMapId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionMaps_HeroId",
                table: "SessionMaps",
                column: "HeroId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SessionMapId",
                table: "Sessions",
                column: "SessionMapId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Edges_HeroMaps_HeroMapId",
                table: "Edges",
                column: "HeroMapId",
                principalTable: "HeroMaps",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Edges_SessionMaps_SessionMapId",
                table: "Edges",
                column: "SessionMapId",
                principalTable: "SessionMaps",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Heroes_HeroMaps_HeroMapId",
                table: "Heroes",
                column: "HeroMapId",
                principalTable: "HeroMaps",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Heroes_Sessions_SessionId",
                table: "Heroes",
                column: "SessionId",
                principalTable: "Sessions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HeroMaps_Planets_HomePlanetId",
                table: "HeroMaps",
                column: "HomePlanetId",
                principalTable: "Planets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Heroes_HeroMaps_HeroMapId",
                table: "Heroes");

            migrationBuilder.DropForeignKey(
                name: "FK_Planets_HeroMaps_HeroMapId",
                table: "Planets");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_SessionMaps_SessionMapId",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "Edges");

            migrationBuilder.DropTable(
                name: "LobbyInfos");

            migrationBuilder.DropTable(
                name: "Lobbies");

            migrationBuilder.DropTable(
                name: "HeroMaps");

            migrationBuilder.DropTable(
                name: "Planets");

            migrationBuilder.DropTable(
                name: "Points");

            migrationBuilder.DropTable(
                name: "SessionMaps");

            migrationBuilder.DropTable(
                name: "Heroes");

            migrationBuilder.DropTable(
                name: "ApplicationUsers");

            migrationBuilder.DropTable(
                name: "Sessions");
        }
    }
}
