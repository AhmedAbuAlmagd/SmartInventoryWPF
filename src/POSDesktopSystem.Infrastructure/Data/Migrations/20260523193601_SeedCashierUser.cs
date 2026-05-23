using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POSDesktopSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedCashierUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 19, 36, 0, 350, DateTimeKind.Utc).AddTicks(7400));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 19, 36, 0, 350, DateTimeKind.Utc).AddTicks(7403));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 19, 36, 0, 350, DateTimeKind.Utc).AddTicks(7406));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 19, 36, 0, 350, DateTimeKind.Utc).AddTicks(7408));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 19, 36, 0, 350, DateTimeKind.Utc).AddTicks(7411));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$abDEjboPYxWg6E/rhm3BmepTjN4hY6jUHkKgECew786cNYQi/syN6");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "IsActive", "PasswordHash", "Role", "UpdatedAt", "Username" },
                values: new object[] { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "$2a$11$YOZJ71qmNMIGB6RITLOUbOr0XiaRLvjR5gPc8RPx2RFksQvwoc9/a", "Cashier", null, "cashier" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 18, 49, 39, 731, DateTimeKind.Utc).AddTicks(5363));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 18, 49, 39, 731, DateTimeKind.Utc).AddTicks(5369));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 18, 49, 39, 731, DateTimeKind.Utc).AddTicks(5373));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 18, 49, 39, 731, DateTimeKind.Utc).AddTicks(5376));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 23, 18, 49, 39, 731, DateTimeKind.Utc).AddTicks(5378));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$FBI.KtTzPU4T4mbnu7SbrubgDGiBRGv2PQfg1NEXtlLHOqw4YnSbS");
        }
    }
}
