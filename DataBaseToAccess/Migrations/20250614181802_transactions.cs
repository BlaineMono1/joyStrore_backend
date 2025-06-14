using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseToAccess.Migrations
{
    /// <inheritdoc />
    public partial class transactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyOrders_LoyaltyTransactionHistories_LoyaltyTransactio~",
                table: "LoyaltyOrders");

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyOrders_LoyaltyTransactionHistoryId",
                table: "LoyaltyOrders");

            migrationBuilder.DropColumn(
                name: "AmountByJoyPlus",
                table: "LoyaltyOrders");

            migrationBuilder.DropColumn(
                name: "LoyaltyTransactionHistoryId",
                table: "LoyaltyOrders");

            migrationBuilder.RenameColumn(
                name: "AmountByRub",
                table: "LoyaltyOrders",
                newName: "AmountPayment");

            migrationBuilder.AddColumn<Guid>(
                name: "LoyaltyTransactionHistoryGuid",
                table: "LoyaltyOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyOrders_LoyaltyTransactionHistoryGuid",
                table: "LoyaltyOrders",
                column: "LoyaltyTransactionHistoryGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyOrders_LoyaltyTransactionHistories_LoyaltyTransactio~",
                table: "LoyaltyOrders",
                column: "LoyaltyTransactionHistoryGuid",
                principalTable: "LoyaltyTransactionHistories",
                principalColumn: "Guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyOrders_LoyaltyTransactionHistories_LoyaltyTransactio~",
                table: "LoyaltyOrders");

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyOrders_LoyaltyTransactionHistoryGuid",
                table: "LoyaltyOrders");

            migrationBuilder.DropColumn(
                name: "LoyaltyTransactionHistoryGuid",
                table: "LoyaltyOrders");

            migrationBuilder.RenameColumn(
                name: "AmountPayment",
                table: "LoyaltyOrders",
                newName: "AmountByRub");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountByJoyPlus",
                table: "LoyaltyOrders",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LoyaltyTransactionHistoryId",
                table: "LoyaltyOrders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyOrders_LoyaltyTransactionHistoryId",
                table: "LoyaltyOrders",
                column: "LoyaltyTransactionHistoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyOrders_LoyaltyTransactionHistories_LoyaltyTransactio~",
                table: "LoyaltyOrders",
                column: "LoyaltyTransactionHistoryId",
                principalTable: "LoyaltyTransactionHistories",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
