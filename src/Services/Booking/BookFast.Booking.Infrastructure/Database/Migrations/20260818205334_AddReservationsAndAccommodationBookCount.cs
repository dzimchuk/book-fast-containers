using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.Booking.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationsAndAccommodationBookCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "book_count",
                schema: "booking",
                table: "accommodations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "reservations",
                schema: "booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    guest_id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    accommodation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tenant_id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    check_in = table.Column<DateOnly>(type: "date", nullable: false),
                    check_out = table.Column<DateOnly>(type: "date", nullable: false),
                    units = table.Column<int>(type: "int", nullable: false),
                    rate_amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    rate_currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_reservations", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_reservations_accommodation_id",
                schema: "booking",
                table: "reservations",
                column: "accommodation_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_guest_id",
                schema: "booking",
                table: "reservations",
                column: "guest_id");

            migrationBuilder.CreateIndex(
                name: "ix_reservations_tenant_id",
                schema: "booking",
                table: "reservations",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reservations",
                schema: "booking");

            migrationBuilder.DropColumn(
                name: "book_count",
                schema: "booking",
                table: "accommodations");
        }
    }
}
