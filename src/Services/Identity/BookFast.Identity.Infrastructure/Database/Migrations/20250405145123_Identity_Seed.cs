using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.Identity.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class Identity_Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"

INSERT INTO [identity].[roles]
(id, [name], normalized_name, concurrency_stamp)
VALUES('01960677-b450-7bc3-a23b-52b6c7c04d76', 'global_admin', 'GLOBAL_ADMIN', null);

INSERT INTO [identity].[roles]
(id, [name], normalized_name, concurrency_stamp)
VALUES('01960677-b45b-778a-b8de-f5c7579c597d', 'tenant_admin', 'TENANT_ADMIN', null);

INSERT INTO [identity].[roles]
(id, [name], normalized_name, concurrency_stamp)
VALUES('01960677-b45b-7c7d-9203-199c657cc7ff', 'tenant_user', 'TENANT_USER', null);

/*Password: P@ssw0rd*/
INSERT INTO [identity].[users]
(id, tenant_id, user_name, normalized_user_name, email, normalized_email, email_confirmed, password_hash, security_stamp, concurrency_stamp, phone_number, phone_number_confirmed, two_factor_enabled, lockout_end, lockout_enabled, access_failed_count)
VALUES('01960679-7f18-7e91-845e-45f8328972c8', null, 'admin@bookfast.com', 'ADMIN@BOOKFAST.COM', 'admin@bookfast.com', 'ADMIN@BOOKFAST.COM', 1, 'AQAAAAIAAYagAAAAEI5pZqeR1Jm+6ILQMsltRi2jTwvUAR0kOcxjAyLM8P5O94rsg0x2O25L76lg4J/tEw==', null, null, null, 0, 0, null, 0, 0);

INSERT INTO [identity].[user_roles]
(user_id, role_id)
VALUES('01960679-7f18-7e91-845e-45f8328972c8', '01960677-b450-7bc3-a23b-52b6c7c04d76');

");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
