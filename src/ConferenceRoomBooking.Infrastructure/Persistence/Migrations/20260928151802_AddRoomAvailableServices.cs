using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferenceRoomBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomAvailableServices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AdditionalServices");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AdditionalServices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "AdditionalServices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalServices_RoomId",
                table: "AdditionalServices",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdditionalServices_Rooms_RoomId",
                table: "AdditionalServices",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdditionalServices_Rooms_RoomId",
                table: "AdditionalServices");

            migrationBuilder.DropIndex(
                name: "IX_AdditionalServices_RoomId",
                table: "AdditionalServices");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "AdditionalServices");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AdditionalServices",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AdditionalServices",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
