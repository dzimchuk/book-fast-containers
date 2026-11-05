using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.Identity.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class OpenIddict_Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""

IF NOT EXISTS (SELECT 1 FROM [identity].[openiddict_scopes] WHERE [name] = 'provider')
BEGIN
    INSERT INTO [identity].[openiddict_scopes] (id, [name], concurrency_token, resources)
    VALUES ('019902f1-2a10-7a55-9c1e-4a2f0c6d8e01', 'provider', NEWID(), '["Identity","PropertyManagement","Search","Booking"]');
END

IF NOT EXISTS (SELECT 1 FROM [identity].[openiddict_scopes] WHERE [name] = 'customer')
BEGIN
    INSERT INTO [identity].[openiddict_scopes] (id, [name], concurrency_token, resources)
    VALUES ('019902f1-2a10-7f3c-8b47-6d5e1b9a2c02', 'customer', NEWID(), '["Booking","Search"]');
END

IF NOT EXISTS (SELECT 1 FROM [identity].[openiddict_applications] WHERE client_id = 'swagger-ui')
BEGIN
    INSERT INTO [identity].[openiddict_applications]
        (id, client_id, client_type, consent_type, display_name, concurrency_token, permissions, requirements, redirect_uris, post_logout_redirect_uris)
    VALUES
        ('019902f1-2a10-7d92-a3f8-8e7c2d4b6a03', 'swagger-ui', 'public', 'implicit', 'Swagger UI client', NEWID(),
         '["ept:authorization","ept:end_session","ept:token","gt:authorization_code","gt:refresh_token","rst:code","scp:email","scp:profile","scp:roles","scp:provider","scp:customer"]',
         '["ft:pkce"]',
         '["https://localhost:5001/swagger/oauth2-redirect.html","https://localhost:5101/swagger/oauth2-redirect.html","https://localhost:5201/swagger/oauth2-redirect.html","https://localhost:5301/swagger/oauth2-redirect.html"]',
         '["https://localhost:5001/","https://localhost:5101/","https://localhost:5201/","https://localhost:5301/"]');
END

IF NOT EXISTS (SELECT 1 FROM [identity].[openiddict_applications] WHERE client_id = 'postman')
BEGIN
    INSERT INTO [identity].[openiddict_applications]
        (id, client_id, client_type, consent_type, display_name, concurrency_token, permissions, requirements, redirect_uris)
    VALUES
        ('019902f1-2a10-74be-b5d1-9f3a7c8e0d04', 'postman', 'public', 'implicit', 'Postman', NEWID(),
         '["ept:authorization","ept:end_session","ept:token","gt:authorization_code","gt:refresh_token","rst:code","scp:email","scp:profile","scp:roles","scp:provider","scp:customer"]',
         '["ft:pkce"]',
         '["https://oauth.pstmn.io/v1/callback"]');
END

""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""

DELETE FROM [identity].[openiddict_applications] WHERE client_id IN ('swagger-ui', 'postman');
DELETE FROM [identity].[openiddict_scopes] WHERE [name] IN ('provider', 'customer');

""");
        }
    }
}
