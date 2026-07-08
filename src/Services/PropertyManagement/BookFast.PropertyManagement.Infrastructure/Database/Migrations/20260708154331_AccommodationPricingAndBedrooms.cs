using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookFast.PropertyManagement.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AccommodationPricingAndBedrooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "bedrooms",
                schema: "property_management",
                table: "accommodations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "max_price_amount",
                schema: "property_management",
                table: "accommodations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "max_price_currency",
                schema: "property_management",
                table: "accommodations",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "min_price_amount",
                schema: "property_management",
                table: "accommodations",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "min_price_currency",
                schema: "property_management",
                table: "accommodations",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            // Backfill: the former single price becomes both MinPrice and MaxPrice.
            // Historical data carried no currency, so USD is used as the backfill default.
            migrationBuilder.Sql(
                """
                UPDATE [property_management].[accommodations]
                SET [min_price_amount] = [price],
                    [min_price_currency] = 'USD',
                    [max_price_amount] = [price],
                    [max_price_currency] = 'USD';
                """);

            migrationBuilder.DropColumn(
                name: "price",
                schema: "property_management",
                table: "accommodations");

            migrationBuilder.DropColumn(
                name: "room_count",
                schema: "property_management",
                table: "accommodations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "price",
                schema: "property_management",
                table: "accommodations",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "room_count",
                schema: "property_management",
                table: "accommodations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE [property_management].[accommodations]
                SET [price] = COALESCE([min_price_amount], [max_price_amount], 0);
                """);

            migrationBuilder.DropColumn(
                name: "bedrooms",
                schema: "property_management",
                table: "accommodations");

            migrationBuilder.DropColumn(
                name: "max_price_amount",
                schema: "property_management",
                table: "accommodations");

            migrationBuilder.DropColumn(
                name: "max_price_currency",
                schema: "property_management",
                table: "accommodations");

            migrationBuilder.DropColumn(
                name: "min_price_amount",
                schema: "property_management",
                table: "accommodations");

            migrationBuilder.DropColumn(
                name: "min_price_currency",
                schema: "property_management",
                table: "accommodations");
        }
    }
}
