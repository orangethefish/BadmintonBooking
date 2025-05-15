using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonBooking.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_Support_For_GoogleMapsApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourtLatitude",
                table: "Facilities",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CourtLongitude",
                table: "Facilities",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MapsUrl",
                table: "Facilities",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourtLatitude",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "CourtLongitude",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "MapsUrl",
                table: "Facilities");
        }
    }
}
