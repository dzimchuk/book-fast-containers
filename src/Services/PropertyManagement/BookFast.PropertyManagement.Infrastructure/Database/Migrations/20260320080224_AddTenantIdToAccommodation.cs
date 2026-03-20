using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.PropertyManagement.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdToAccommodation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tenant_id",
                schema: "property_management",
                table: "accommodations",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE a
                SET a.tenant_id = p.tenant_id
                FROM property_management.accommodations a
                INNER JOIN property_management.properties p ON a.property_id = p.id
                WHERE a.tenant_id = ''
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tenant_id",
                schema: "property_management",
                table: "accommodations");
        }
    }
}
