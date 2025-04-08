using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.Identity.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class OpenIddict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "openiddict_applications",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    application_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    client_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    client_secret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    client_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    concurrency_token = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    consent_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    display_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    display_names = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    json_web_key_set = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    permissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    post_logout_redirect_uris = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    redirect_uris = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    settings = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_openiddict_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "openiddict_scopes",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    concurrency_token = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    descriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    display_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    display_names = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resources = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_openiddict_scopes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "openiddict_authorizations",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    application_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    concurrency_token = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    creation_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_openiddict_authorizations", x => x.id);
                    table.ForeignKey(
                        name: "fk_openiddict_authorizations_openiddict_applications_application_id",
                        column: x => x.application_id,
                        principalSchema: "identity",
                        principalTable: "openiddict_applications",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "openiddict_tokens",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    application_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    authorization_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    concurrency_token = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    creation_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expiration_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    redemption_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    reference_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_openiddict_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_openiddict_tokens_openiddict_applications_application_id",
                        column: x => x.application_id,
                        principalSchema: "identity",
                        principalTable: "openiddict_applications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_openiddict_tokens_openiddict_authorizations_authorization_id",
                        column: x => x.authorization_id,
                        principalSchema: "identity",
                        principalTable: "openiddict_authorizations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_openiddict_applications_client_id",
                schema: "identity",
                table: "openiddict_applications",
                column: "client_id",
                unique: true,
                filter: "[client_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_openiddict_authorizations_application_id_status_subject_type",
                schema: "identity",
                table: "openiddict_authorizations",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_openiddict_scopes_name",
                schema: "identity",
                table: "openiddict_scopes",
                column: "name",
                unique: true,
                filter: "[name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_openiddict_tokens_application_id_status_subject_type",
                schema: "identity",
                table: "openiddict_tokens",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_openiddict_tokens_authorization_id",
                schema: "identity",
                table: "openiddict_tokens",
                column: "authorization_id");

            migrationBuilder.CreateIndex(
                name: "ix_openiddict_tokens_reference_id",
                schema: "identity",
                table: "openiddict_tokens",
                column: "reference_id",
                unique: true,
                filter: "[reference_id] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "openiddict_scopes",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "openiddict_tokens",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "openiddict_authorizations",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "openiddict_applications",
                schema: "identity");
        }
    }
}
