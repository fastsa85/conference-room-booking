using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferenceRoomBooking.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingAdditionalServiceRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingAdditionalServices_AdditionalServices_AdditionalServiceId",
                table: "BookingAdditionalServices");

            migrationBuilder.AddColumn<Guid>(
                name: "BookingId",
                table: "BookingAdditionalServices",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_BookingAdditionalServices_BookingId",
                table: "BookingAdditionalServices",
                column: "BookingId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingAdditionalServices_AdditionalServices_AdditionalServiceId",
                table: "BookingAdditionalServices",
                column: "AdditionalServiceId",
                principalTable: "AdditionalServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookingAdditionalServices_Bookings_BookingId",
                table: "BookingAdditionalServices",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingAdditionalServices_AdditionalServices_AdditionalServiceId",
                table: "BookingAdditionalServices");

            migrationBuilder.DropForeignKey(
                name: "FK_BookingAdditionalServices_Bookings_BookingId",
                table: "BookingAdditionalServices");

            migrationBuilder.DropIndex(
                name: "IX_BookingAdditionalServices_BookingId",
                table: "BookingAdditionalServices");

            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "BookingAdditionalServices");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingAdditionalServices_AdditionalServices_AdditionalServiceId",
                table: "BookingAdditionalServices",
                column: "AdditionalServiceId",
                principalTable: "AdditionalServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
