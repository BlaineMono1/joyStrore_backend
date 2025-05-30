using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseToAccess.Migrations
{
    /// <inheritdoc />
    public partial class addonfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddOns_GroupAddOns_GroupAddOnGuid",
                table: "AddOns");

            migrationBuilder.RenameColumn(
                name: "GroupAddOnGuid",
                table: "AddOns",
                newName: "GroupAddOnId");

            migrationBuilder.RenameIndex(
                name: "IX_AddOns_GroupAddOnGuid",
                table: "AddOns",
                newName: "IX_AddOns_GroupAddOnId");

            migrationBuilder.AddForeignKey(
                name: "FK_AddOns_GroupAddOns_GroupAddOnId",
                table: "AddOns",
                column: "GroupAddOnId",
                principalTable: "GroupAddOns",
                principalColumn: "Guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddOns_GroupAddOns_GroupAddOnId",
                table: "AddOns");

            migrationBuilder.RenameColumn(
                name: "GroupAddOnId",
                table: "AddOns",
                newName: "GroupAddOnGuid");

            migrationBuilder.RenameIndex(
                name: "IX_AddOns_GroupAddOnId",
                table: "AddOns",
                newName: "IX_AddOns_GroupAddOnGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_AddOns_GroupAddOns_GroupAddOnGuid",
                table: "AddOns",
                column: "GroupAddOnGuid",
                principalTable: "GroupAddOns",
                principalColumn: "Guid");
        }
    }
}
