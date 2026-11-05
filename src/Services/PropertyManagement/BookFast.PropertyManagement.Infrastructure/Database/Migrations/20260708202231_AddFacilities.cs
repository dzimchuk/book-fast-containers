using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.PropertyManagement.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "facilities",
                schema: "property_management",
                table: "properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "facilities",
                schema: "property_management",
                table: "accommodations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "facilities",
                schema: "property_management",
                table: "properties");

            migrationBuilder.DropColumn(
                name: "facilities",
                schema: "property_management",
                table: "accommodations");
        }
    }
}
