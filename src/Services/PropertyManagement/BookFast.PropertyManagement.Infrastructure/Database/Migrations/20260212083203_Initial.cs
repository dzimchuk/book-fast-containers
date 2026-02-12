using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.PropertyManagement.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "property_management");

            migrationBuilder.CreateSequence(
                name: "accommodationseq",
                schema: "property_management");

            migrationBuilder.CreateSequence(
                name: "propertyseq",
                schema: "property_management");

            migrationBuilder.CreateTable(
                name: "properties",
                schema: "property_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    tenant_id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    images = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address_country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    address_state = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    address_city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    address_street = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    address_zip_code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    location_latitude = table.Column<double>(type: "float", nullable: true),
                    location_longitude = table.Column<double>(type: "float", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_properties", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "accommodations",
                schema: "property_management",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    property_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    room_count = table.Column<int>(type: "int", nullable: false),
                    images = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accommodations", x => x.id);
                    table.ForeignKey(
                        name: "fk_accommodations_properties_property_id",
                        column: x => x.property_id,
                        principalSchema: "property_management",
                        principalTable: "properties",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_accommodations_property_id",
                schema: "property_management",
                table: "accommodations",
                column: "property_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accommodations",
                schema: "property_management");

            migrationBuilder.DropTable(
                name: "properties",
                schema: "property_management");

            migrationBuilder.DropSequence(
                name: "accommodationseq",
                schema: "property_management");

            migrationBuilder.DropSequence(
                name: "propertyseq",
                schema: "property_management");
        }
    }
}
