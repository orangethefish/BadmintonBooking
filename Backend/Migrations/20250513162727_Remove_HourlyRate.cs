using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonBooking.API.Migrations
{
    /// <inheritdoc />
    public partial class Remove_HourlyRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "PricingConfigurations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "PricingConfigurations",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
