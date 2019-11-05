using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookFast.Booking.Data.Migrations
{
    public partial class BookingService_001 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "booking");

            migrationBuilder.CreateTable(
                name: "BookingRecords",
                schema: "booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    User = table.Column<string>(nullable: false),
                    AccommodationId = table.Column<int>(nullable: false),
                    AccommodationName = table.Column<string>(maxLength: 320, nullable: false),
                    FacilityId = table.Column<int>(nullable: false),
                    FacilityName = table.Column<string>(maxLength: 320, nullable: false),
                    StreetAddress = table.Column<string>(nullable: true),
                    FromDate = table.Column<DateTimeOffset>(nullable: false),
                    ToDate = table.Column<DateTimeOffset>(nullable: false),
                    CanceledOn = table.Column<DateTimeOffset>(nullable: true),
                    CheckedInOn = table.Column<DateTimeOffset>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                schema: "booking",
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingRecords",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "Events",
                schema: "booking");
        }
    }
}
