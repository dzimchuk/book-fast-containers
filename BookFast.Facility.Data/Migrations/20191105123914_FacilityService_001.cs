using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookFast.Facility.Data.Migrations
{
    public partial class FacilityService_001 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "facility");

            migrationBuilder.CreateSequence(
                name: "accommodationseq",
                schema: "facility",
                incrementBy: 10);

            migrationBuilder.CreateSequence(
                name: "facilityseq",
                schema: "facility",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "facility",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    EventName = table.Column<string>(maxLength: 100, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(nullable: false),
                    User = table.Column<string>(maxLength: 50, nullable: false),
                    Tenant = table.Column<string>(maxLength: 50, nullable: false),
                    Payload = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Facilities",
                schema: "facility",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 320, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Owner = table.Column<string>(nullable: false),
                    StreetAddress = table.Column<string>(nullable: true),
                    Longitude = table.Column<double>(nullable: true),
                    Latitude = table.Column<double>(nullable: true),
                    Images = table.Column<string>(nullable: true)
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
                    Id = table.Column<int>(nullable: false),
                    FacilityId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 320, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    RoomCount = table.Column<int>(nullable: false),
                    Images = table.Column<string>(nullable: true)
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_FacilityId",
                schema: "facility",
                table: "Accommodations",
                column: "FacilityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accommodations",
                schema: "facility");

            migrationBuilder.DropTable(
                name: "Events",
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
