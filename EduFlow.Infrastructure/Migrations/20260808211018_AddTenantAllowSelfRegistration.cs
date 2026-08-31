using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantAllowSelfRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowSelfRegistration",
                table: "Tenants",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowSelfRegistration",
                table: "Tenants");
        }
    }
}
