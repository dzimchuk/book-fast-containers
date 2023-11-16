using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Identity_003_Seed_OpenIddict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
INSERT [identity].[OpenIddictApplications] ([Id], [ClientId], [ClientSecret], [ConcurrencyToken], [ConsentType], [DisplayName], [DisplayNames], [Permissions], [PostLogoutRedirectUris], [Properties], [RedirectUris], [Requirements], [Type]) 
VALUES (N'049d229e-a535-4c5c-9809-aef86b77d5b1', 
N'postman', 
NULL, 
N'68e365b0-7cee-4bc0-98e9-e861d8d67862', 
N'implicit', 
N'Postman', 
NULL, 
N'[""ept:authorization"",""ept:logout"",""ept:token"",""gt:authorization_code"",""gt:refresh_token"",""rst:code"",""scp:email"",""scp:profile"",""scp:roles"",""scp:all"",""scp:booking"",""scp:property-management""]', 
NULL, 
NULL, 
N'[""https://oauth.pstmn.io/v1/callback""]', 
N'[""ft:pkce""]', 
N'public');

INSERT [identity].[OpenIddictApplications] ([Id], [ClientId], [ClientSecret], [ConcurrencyToken], [ConsentType], [DisplayName], [DisplayNames], [Permissions], [PostLogoutRedirectUris], [Properties], [RedirectUris], [Requirements], [Type]) 
VALUES (N'6568e06f-cefe-48bf-9993-18e3dd137355', 
N'bookfast-client', 
NULL, 
N'f343896b-28dc-41cc-8872-fe5a02ce5e5a', 
N'implicit', N'BookFast client application', 
NULL, 
N'[""ept:authorization"",""ept:logout"",""ept:token"",""gt:authorization_code"",""gt:refresh_token"",""rst:code"",""scp:email"",""scp:profile"",""scp:roles"",""scp:booking""]', 
N'[""https://localhost:7288/authentication/logout-callback""]', 
NULL, 
N'[""https://localhost:7288/authentication/login-callback""]', 
N'[""ft:pkce""]', 
N'public');

INSERT [identity].[OpenIddictApplications] ([Id], [ClientId], [ClientSecret], [ConcurrencyToken], [ConsentType], [DisplayName], [DisplayNames], [Permissions], [PostLogoutRedirectUris], [Properties], [RedirectUris], [Requirements], [Type]) 
VALUES (N'878f3caa-a124-4128-a049-0bd11e64ab11', 
N'service-client', 
N'AQAAAAEAACcQAAAAEFwHFb6Q82qbHfYwUeiSVSt23SQcPyfLq0mEN0bj6HPjIQP3+ZIZOXNUE2Ul8+qi1A==', 
N'a30a761b-6a39-4df1-b50b-ad847dc9c641', 
NULL, 
N'Backend service client', 
NULL, 
N'[""ept:token"",""gt:client_credentials"",""scp:booking"",""scp:property-management"",""rst:token""]', 
NULL, NULL, NULL, NULL, 
N'confidential');

INSERT [identity].[OpenIddictApplications] ([Id], [ClientId], [ClientSecret], [ConcurrencyToken], [ConsentType], [DisplayName], [DisplayNames], [Permissions], [PostLogoutRedirectUris], [Properties], [RedirectUris], [Requirements], [Type]) 
VALUES (N'a5e00358-e68b-4e41-ab50-ec7e23c354db', 
N'bookfast-admin', 
NULL, 
N'd8db1b57-172a-4114-b7aa-29f5271b75eb', 
N'implicit', 
N'BookFast admin application', 
NULL, 
N'[""ept:authorization"",""ept:logout"",""ept:token"",""gt:authorization_code"",""gt:refresh_token"",""rst:code"",""scp:email"",""scp:profile"",""scp:roles"",""scp:all""]', 
N'[""https://localhost:7200/authentication/logout-callback""]', 
NULL, 
N'[""https://localhost:7200/authentication/login-callback""]', 
N'[""ft:pkce""]', 
N'public');

INSERT [identity].[OpenIddictScopes] ([Id], [ConcurrencyToken], [Description], [Descriptions], [DisplayName], [DisplayNames], [Name], [Properties], [Resources]) 
VALUES (N'18eedb61-a10a-4953-808c-b0bd6c49bf69', 
N'27b20b7b-3357-43e0-a23c-12fdee892227', 
NULL, NULL, NULL, NULL, 
N'booking', 
NULL, 
N'[""booking-api""]');

INSERT [identity].[OpenIddictScopes] ([Id], [ConcurrencyToken], [Description], [Descriptions], [DisplayName], [DisplayNames], [Name], [Properties], [Resources]) 
VALUES (N'26b8feb9-12f5-46b9-b47d-f6c687e03af4', 
N'96328d13-5e7d-4ed4-b95f-9780f8fccbf7', 
NULL, NULL, NULL, NULL, 
N'property-management', 
NULL, 
N'[""property-management-api""]');

INSERT [identity].[OpenIddictScopes] ([Id], [ConcurrencyToken], [Description], [Descriptions], [DisplayName], [DisplayNames], [Name], [Properties], [Resources]) 
VALUES (N'c49d1af4-15d3-42a5-8778-7cf50b7c933e', 
N'd62f75c3-9e66-46f7-8cdb-91ae0bd04575', 
NULL, NULL, NULL, NULL, 
N'all', 
NULL, 
N'[""booking-api"",""property-management-api""]');


");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
