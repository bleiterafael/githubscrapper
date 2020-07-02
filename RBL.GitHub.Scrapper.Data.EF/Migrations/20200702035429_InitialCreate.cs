using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RBL.GitHub.Scrapper.Data.EF.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScrappingInfos",
                columns: table => new
                {
                    ScrappingInfoId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GitHubRepository = table.Column<string>(nullable: true),
                    TotalFiles = table.Column<int>(nullable: false),
                    TotalLines = table.Column<int>(nullable: false),
                    TotalSize = table.Column<decimal>(nullable: false),
                    TotalSizeDescription = table.Column<string>(nullable: true),
                    LastUpdate = table.Column<DateTime>(nullable: false),
                    ProcessTime = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrappingInfos", x => x.ScrappingInfoId);
                });

            migrationBuilder.CreateTable(
                name: "ScrappingInfoExtension",
                columns: table => new
                {
                    ScrappingInfoExtensionId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScrappingInfoId = table.Column<int>(nullable: false),
                    Extension = table.Column<string>(nullable: true),
                    TotalFiles = table.Column<int>(nullable: false),
                    TotalLines = table.Column<int>(nullable: false),
                    TotalSize = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScrappingInfoExtension", x => x.ScrappingInfoExtensionId);
                    table.ForeignKey(
                        name: "FK_ScrappingInfoExtension_ScrappingInfos_ScrappingInfoId",
                        column: x => x.ScrappingInfoId,
                        principalTable: "ScrappingInfos",
                        principalColumn: "ScrappingInfoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScrappingInfoExtension_ScrappingInfoId",
                table: "ScrappingInfoExtension",
                column: "ScrappingInfoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScrappingInfoExtension");

            migrationBuilder.DropTable(
                name: "ScrappingInfos");
        }
    }
}
