using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace POSDesktopSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Barcode", "Category", "CostPrice", "CreatedAt", "Description", "IsActive", "Name", "Price", "StockQuantity", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "7501031311309", "Beverages", 12.00m, new DateTime(2026, 5, 23, 18, 49, 39, 731, DateTimeKind.Utc).AddTicks(5363), null, true, "Coca Cola 600ml", 18.50m, 50, null },
                    { 2, "7501031311310", "Beverages", 11.00m, new DateTime(2026, 5, 23, 18, 49, 39, 731, DateTimeKind.Utc).AddTicks(5369), null, true, "Pepsi 600ml", 17.00m, 40, null },
                    { 3, "1234567890123", "Snacks", 8.50m, new DateTime(2026, 5, 23, 18, 49, 39, 731, DateTimeKind.Utc).AddTicks(5373), null, true, "Lays Classic", 15.00m, 100, null },
                    { 4, "9876543210987", "Dairy", 20.00m, new DateTime(2026, 5, 23, 18, 49, 39, 731, DateTimeKind.Utc).AddTicks(5376), null, true, "Milk 1L", 25.00m, 30, null },
                    { 5, "1111111111111", "Bakery", 28.00m, new DateTime(2026, 5, 23, 18, 49, 39, 731, DateTimeKind.Utc).AddTicks(5378), null, true, "White Bread", 35.00m, 20, null }
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$FBI.KtTzPU4T4mbnu7SbrubgDGiBRGv2PQfg1NEXtlLHOqw4YnSbS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$uVFCHSJsx5JRwjKT.KPv/uzCc43SVCDHT8qSU7xpi9iut3zZP7OiC");
        }
    }
}
