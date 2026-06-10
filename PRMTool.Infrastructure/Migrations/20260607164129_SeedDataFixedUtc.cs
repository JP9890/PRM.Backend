using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRMTool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDataFixedUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 6, 7, 16, 37, 20, 278, DateTimeKind.Utc).AddTicks(7580), new DateTime(2026, 6, 7, 16, 37, 20, 278, DateTimeKind.Utc).AddTicks(8069) });
        }
    }
}
