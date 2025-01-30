using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseToAccess.Migrations
{
    /// <inheritdoc />
    public partial class Test2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ProductsProductItems_ProductTransactionItemId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductsProductItems_ProductTransactionHistories_ProductTra~",
                table: "ProductsProductItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductsProductItems",
                table: "ProductsProductItems");

            migrationBuilder.RenameTable(
                name: "ProductsProductItems",
                newName: "ProductTransactionItems");

            migrationBuilder.RenameIndex(
                name: "IX_ProductsProductItems_ProductTransactionHistoryId",
                table: "ProductTransactionItems",
                newName: "IX_ProductTransactionItems_ProductTransactionHistoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductTransactionItems",
                table: "ProductTransactionItems",
                column: "Guid");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ProductTransactionItems_ProductTransactionItemId",
                table: "Orders",
                column: "ProductTransactionItemId",
                principalTable: "ProductTransactionItems",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTransactionItems_ProductTransactionHistories_Product~",
                table: "ProductTransactionItems",
                column: "ProductTransactionHistoryId",
                principalTable: "ProductTransactionHistories",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ProductTransactionItems_ProductTransactionItemId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTransactionItems_ProductTransactionHistories_Product~",
                table: "ProductTransactionItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductTransactionItems",
                table: "ProductTransactionItems");

            migrationBuilder.RenameTable(
                name: "ProductTransactionItems",
                newName: "ProductsProductItems");

            migrationBuilder.RenameIndex(
                name: "IX_ProductTransactionItems_ProductTransactionHistoryId",
                table: "ProductsProductItems",
                newName: "IX_ProductsProductItems_ProductTransactionHistoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductsProductItems",
                table: "ProductsProductItems",
                column: "Guid");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ProductsProductItems_ProductTransactionItemId",
                table: "Orders",
                column: "ProductTransactionItemId",
                principalTable: "ProductsProductItems",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductsProductItems_ProductTransactionHistories_ProductTra~",
                table: "ProductsProductItems",
                column: "ProductTransactionHistoryId",
                principalTable: "ProductTransactionHistories",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
