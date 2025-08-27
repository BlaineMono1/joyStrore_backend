using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseToAccess.Migrations
{
    /// <inheritdoc />
    public partial class Variety_for_Sub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Variety",
                table: "Subscriptions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Variety",
                table: "Subscriptions");
        }
    }
}
