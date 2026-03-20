using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.PropertyManagement.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantIdIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_properties_tenant_id",
                schema: "property_management",
                table: "properties",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_accommodations_tenant_id",
                schema: "property_management",
                table: "accommodations",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_properties_tenant_id",
                schema: "property_management",
                table: "properties");

            migrationBuilder.DropIndex(
                name: "ix_accommodations_tenant_id",
                schema: "property_management",
                table: "accommodations");
        }
    }
}
