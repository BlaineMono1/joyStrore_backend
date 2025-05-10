using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataBaseToAccess.Migrations
{
    /// <inheritdoc />
    public partial class newProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DiscountPercent",
                table: "Products",
                newName: "DiscountPercentUa");

            migrationBuilder.RenameColumn(
                name: "DiscountDate",
                table: "Products",
                newName: "DiscountDateUa");

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountDateTr",
                table: "Products",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountPercentTr",
                table: "Products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPreOrder",
                table: "Editions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountDateTr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DiscountPercentTr",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsPreOrder",
                table: "Editions");

            migrationBuilder.RenameColumn(
                name: "DiscountPercentUa",
                table: "Products",
                newName: "DiscountPercent");

            migrationBuilder.RenameColumn(
                name: "DiscountDateUa",
                table: "Products",
                newName: "DiscountDate");
        }
    }
}
