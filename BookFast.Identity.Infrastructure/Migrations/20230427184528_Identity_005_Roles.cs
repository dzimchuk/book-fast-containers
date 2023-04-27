using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.Identity.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Identity_005_Roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
 INSERT [identity].[AspNetRoles] 
 (
     [Id], 
     [Name], 
     [NormalizedName], 
     [ConcurrencyStamp]
 ) 
 VALUES 
 (
     N'18fa94de-df5f-480f-98d4-e5cd6ac12de8', 
     N'tenant-admin', 
     N'TENANT-ADMIN', 
     N'66222204-d94a-4836-85fc-1f2557f93c45'
 );

INSERT [identity].[AspNetRoles] 
 (
     [Id], 
     [Name], 
     [NormalizedName], 
     [ConcurrencyStamp]
 ) 
 VALUES 
 (
     N'6d6447a7-ed4e-4f34-a989-1185168469dd', 
     N'tenant-user', 
     N'TENANT-USER', 
     N'bef1fdb8-e614-457d-81cc-83d1cc5cc978'
 );
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
