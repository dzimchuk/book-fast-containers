using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.Identity.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class CustomerRole_Seed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""

IF NOT EXISTS (SELECT 1 FROM [identity].[roles] WHERE [name] = 'customer')
BEGIN
    INSERT INTO [identity].[roles] (id, [name], normalized_name, concurrency_stamp)
    VALUES ('019b0a1e-3c40-7d2a-8e51-6f4b7a9c1d03', 'customer', 'CUSTOMER', null);
END

""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""

DELETE FROM [identity].[user_roles] WHERE role_id = '019b0a1e-3c40-7d2a-8e51-6f4b7a9c1d03';
DELETE FROM [identity].[roles] WHERE id = '019b0a1e-3c40-7d2a-8e51-6f4b7a9c1d03';

""");
        }
    }
}
