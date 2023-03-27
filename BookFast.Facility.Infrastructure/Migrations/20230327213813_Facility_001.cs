using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.Facility.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Facility_001 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "facility");

            migrationBuilder.CreateSequence(
                name: "accommodationseq",
                schema: "facility");

            migrationBuilder.CreateSequence(
                name: "facilityseq",
                schema: "facility");

            migrationBuilder.CreateTable(
                name: "Facilities",
                schema: "facility",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Owner = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StreetAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location_Latitude = table.Column<double>(type: "float", nullable: true),
                    Location_Longitude = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accommodations",
                schema: "facility",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    FacilityId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomCount = table.Column<int>(type: "int", nullable: false),
                    Images = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accommodations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accommodations_Facilities_FacilityId",
                        column: x => x.FacilityId,
                        principalSchema: "facility",
                        principalTable: "Facilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_FacilityId",
                schema: "facility",
                table: "Accommodations",
                column: "FacilityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accommodations",
                schema: "facility");

            migrationBuilder.DropTable(
                name: "Facilities",
                schema: "facility");

            migrationBuilder.DropSequence(
                name: "accommodationseq",
                schema: "facility");

            migrationBuilder.DropSequence(
                name: "facilityseq",
                schema: "facility");
        }
    }
}
