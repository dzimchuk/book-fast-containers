using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.Booking.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payment_attempts",
                schema: "booking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reservation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    due_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_attempts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payment_attempts_reservation_id",
                schema: "booking",
                table: "payment_attempts",
                column: "reservation_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_attempts",
                schema: "booking");
        }
    }
}
