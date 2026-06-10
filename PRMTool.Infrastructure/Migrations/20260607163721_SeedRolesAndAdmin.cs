using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PRMTool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedRolesAndAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Administrator role with full permissions", "Admin" },
                    { 2, "Manager role for managing projects and resources", "Manager" },
                    { 3, "Employee role for submitting timesheets", "Employee" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "IsActive", "PasswordHash", "RequiresPasswordChange", "RoleId", "UpdatedAt", "Username" },
                values: new object[] { 1, new DateTime(2026, 6, 7, 16, 37, 20, 278, DateTimeKind.Utc).AddTicks(7580), "admin@prm.local", true, "dummy_hash_for_admin_123", false, 1, new DateTime(2026, 6, 7, 16, 37, 20, 278, DateTimeKind.Utc).AddTicks(8069), "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
