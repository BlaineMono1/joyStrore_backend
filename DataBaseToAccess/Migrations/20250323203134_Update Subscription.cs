using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseToAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceSettings_SubscriptionId",
                table: "PriceSettings");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSettings_SubscriptionId",
                table: "PriceSettings",
                column: "SubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PriceSettings_SubscriptionId",
                table: "PriceSettings");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSettings_SubscriptionId",
                table: "PriceSettings",
                column: "SubscriptionId",
                unique: true);
        }
    }
}
