using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonBooking.API.Migrations
{
    /// <inheritdoc />
    public partial class Add_OwnerId_To_Facility_And_Court : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "PricingConfigurations",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Facilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Courts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "BookingLocks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Facilities_OwnerId",
                table: "Facilities",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Courts_OwnerId",
                table: "Courts",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingLocks_UserId",
                table: "BookingLocks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingLocks_Users_UserId",
                table: "BookingLocks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Courts_Users_OwnerId",
                table: "Courts",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Facilities_Users_OwnerId",
                table: "Facilities",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingLocks_Users_UserId",
                table: "BookingLocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Courts_Users_OwnerId",
                table: "Courts");

            migrationBuilder.DropForeignKey(
                name: "FK_Facilities_Users_OwnerId",
                table: "Facilities");

            migrationBuilder.DropIndex(
                name: "IX_Facilities_OwnerId",
                table: "Facilities");

            migrationBuilder.DropIndex(
                name: "IX_Courts_OwnerId",
                table: "Courts");

            migrationBuilder.DropIndex(
                name: "IX_BookingLocks_UserId",
                table: "BookingLocks");

            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "PricingConfigurations");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Facilities");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BookingLocks");
        }
    }
}
