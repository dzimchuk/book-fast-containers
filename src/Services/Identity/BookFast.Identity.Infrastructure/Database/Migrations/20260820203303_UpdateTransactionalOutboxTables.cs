using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.Identity.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransactionalOutboxTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outbox_state_created",
                schema: "identity",
                table: "outbox_state");

            migrationBuilder.DropIndex(
                name: "ix_outbox_message_enqueue_time",
                schema: "identity",
                table: "outbox_message");

            migrationBuilder.DropIndex(
                name: "ix_outbox_message_expiration_time",
                schema: "identity",
                table: "outbox_message");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_outbox_state_created",
                schema: "identity",
                table: "outbox_state",
                column: "created");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_enqueue_time",
                schema: "identity",
                table: "outbox_message",
                column: "enqueue_time");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_expiration_time",
                schema: "identity",
                table: "outbox_message",
                column: "expiration_time");
        }
    }
}
