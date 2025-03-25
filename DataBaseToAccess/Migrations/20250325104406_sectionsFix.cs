using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseToAccess.Migrations
{
    /// <inheritdoc />
    public partial class sectionsFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Editions_Sections_SectionGuid",
                table: "Editions");

            migrationBuilder.DropIndex(
                name: "IX_Editions_SectionGuid",
                table: "Editions");

            migrationBuilder.DropColumn(
                name: "SectionGuid",
                table: "Editions");

            migrationBuilder.CreateTable(
                name: "SectionsEditions",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uuid", nullable: false),
                    EdtitonId = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateCreate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionsEditions", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_SectionsEditions_Editions_EditionId",
                        column: x => x.EdtitonId,
                        principalTable: "Editions",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SectionsEditions_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SectionsEditions_EditionGuid",
                table: "SectionsEditions",
                column: "EditionGuid");

            migrationBuilder.CreateIndex(
                name: "IX_SectionsEditions_SectionId",
                table: "SectionsEditions",
                column: "SectionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SectionsEditions");

            migrationBuilder.AddColumn<Guid>(
                name: "SectionGuid",
                table: "Editions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Editions_SectionGuid",
                table: "Editions",
                column: "SectionGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Editions_Sections_SectionGuid",
                table: "Editions",
                column: "SectionGuid",
                principalTable: "Sections",
                principalColumn: "Guid");
        }
    }
}
