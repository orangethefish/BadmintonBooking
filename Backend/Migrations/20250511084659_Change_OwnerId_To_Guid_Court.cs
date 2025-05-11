using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BadmintonBooking.API.Migrations
{
    /// <inheritdoc />
    public partial class Change_OwnerId_To_Guid_Court : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courts_Users_OwnerId1",
                table: "Courts");

            migrationBuilder.DropIndex(
                name: "IX_Courts_OwnerId1",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "OwnerId1",
                table: "Courts");

            migrationBuilder.AlterColumn<Guid>(
                name: "OwnerId",
                table: "Courts",
                type: "char(36)",
                nullable: false,
                collation: "ascii_general_ci",
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Courts_OwnerId",
                table: "Courts",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courts_Users_OwnerId",
                table: "Courts",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courts_Users_OwnerId",
                table: "Courts");

            migrationBuilder.DropIndex(
                name: "IX_Courts_OwnerId",
                table: "Courts");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "Courts",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId1",
                table: "Courts",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Courts_OwnerId1",
                table: "Courts",
                column: "OwnerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Courts_Users_OwnerId1",
                table: "Courts",
                column: "OwnerId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
