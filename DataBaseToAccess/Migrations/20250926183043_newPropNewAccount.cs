using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseToAccess.Migrations
{
    /// <inheritdoc />
    public partial class newPropNewAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NewAccount",
                table: "Orders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewAccount",
                table: "Orders");
        }
    }
}
