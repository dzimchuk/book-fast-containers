using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.Identity.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class TransactionalOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbox_state",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    message_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    consumer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    lock_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    received = table.Column<DateTime>(type: "datetime2", nullable: false),
                    receive_count = table.Column<int>(type: "int", nullable: false),
                    expiration_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    consumed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delivered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_sequence_number = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_state", x => x.id);
                    table.UniqueConstraint("ak_inbox_state_message_id_consumer_id", x => new { x.message_id, x.consumer_id });
                });

            migrationBuilder.CreateTable(
                name: "outbox_state",
                schema: "identity",
                columns: table => new
                {
                    outbox_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    lock_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    row_version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    delivered = table.Column<DateTime>(type: "datetime2", nullable: true),
                    last_sequence_number = table.Column<long>(type: "bigint", nullable: true),
                    bus_name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_state", x => x.outbox_id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_message",
                schema: "identity",
                columns: table => new
                {
                    sequence_number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enqueue_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    sent_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    headers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    inbox_message_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    inbox_consumer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    outbox_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    message_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    content_type = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    message_type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    correlation_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    initiator_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    request_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    source_address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    destination_address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    response_address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    fault_address = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    expiration_time = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_message", x => x.sequence_number);
                    table.ForeignKey(
                        name: "fk_outbox_message_inbox_state_inbox_message_id_inbox_consumer_id",
                        columns: x => new { x.inbox_message_id, x.inbox_consumer_id },
                        principalSchema: "identity",
                        principalTable: "inbox_state",
                        principalColumns: new[] { "message_id", "consumer_id" });
                    table.ForeignKey(
                        name: "fk_outbox_message_outbox_state_outbox_id",
                        column: x => x.outbox_id,
                        principalSchema: "identity",
                        principalTable: "outbox_state",
                        principalColumn: "outbox_id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_inbox_state_delivered",
                schema: "identity",
                table: "inbox_state",
                column: "delivered");

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

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_inbox_message_id_inbox_consumer_id_sequence_number",
                schema: "identity",
                table: "outbox_message",
                columns: new[] { "inbox_message_id", "inbox_consumer_id", "sequence_number" },
                unique: true,
                filter: "[inbox_message_id] IS NOT NULL AND [inbox_consumer_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_outbox_id_sequence_number",
                schema: "identity",
                table: "outbox_message",
                columns: new[] { "outbox_id", "sequence_number" },
                unique: true,
                filter: "[outbox_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_state_bus_name_created",
                schema: "identity",
                table: "outbox_state",
                columns: new[] { "bus_name", "created" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_state_created",
                schema: "identity",
                table: "outbox_state",
                column: "created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_message",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "inbox_state",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "outbox_state",
                schema: "identity");
        }
    }
}
