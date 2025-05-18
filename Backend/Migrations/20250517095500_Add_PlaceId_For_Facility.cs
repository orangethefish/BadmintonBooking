using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonBooking.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_PlaceId_For_Facility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlaceId",
                table: "Facilities",
                type: "varchar(36)",
                maxLength: 36,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaceId",
                table: "Facilities");
        }
    }
}
