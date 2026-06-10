using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PRMTool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$n6eLlN0jG.5EUq6tmN5eG.ULhugVu4L7o8JVWy8MT6/Qe/kpPvqm6");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "dummy_hash_for_admin_123");
        }
    }
}
