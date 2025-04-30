using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseToAccess.Migrations
{
    /// <inheritdoc />
    public partial class new_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ProductTransactionItems_ProductTransactionItemId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ProductTransactionItemId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductTransactionItemId",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductTransactionItemGuid",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProductTransactionItemGuid",
                table: "Orders",
                column: "ProductTransactionItemGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ProductTransactionItems_ProductTransactionItemGuid",
                table: "Orders",
                column: "ProductTransactionItemGuid",
                principalTable: "ProductTransactionItems",
                principalColumn: "Guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ProductTransactionItems_ProductTransactionItemGuid",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ProductTransactionItemGuid",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductTransactionItemGuid",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductTransactionItemId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProductTransactionItemId",
                table: "Orders",
                column: "ProductTransactionItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ProductTransactionItems_ProductTransactionItemId",
                table: "Orders",
                column: "ProductTransactionItemId",
                principalTable: "ProductTransactionItems",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
