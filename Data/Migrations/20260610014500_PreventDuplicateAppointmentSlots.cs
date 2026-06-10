using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BarberBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicateAppointmentSlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM AppointmentSlots
                WHERE Id IN (
                    SELECT slot.Id
                    FROM AppointmentSlots AS slot
                    LEFT JOIN Reservations AS reservation
                        ON reservation.AppointmentSlotId = slot.Id
                    WHERE reservation.Id IS NULL
                        AND slot.Id <> (
                            SELECT COALESCE(
                                MIN(reservedSlot.Id),
                                MIN(anySlot.Id)
                            )
                            FROM AppointmentSlots AS anySlot
                            LEFT JOIN Reservations AS reservedReservation
                                ON reservedReservation.AppointmentSlotId = anySlot.Id
                            LEFT JOIN AppointmentSlots AS reservedSlot
                                ON reservedSlot.Id = anySlot.Id
                                AND reservedReservation.Id IS NOT NULL
                            WHERE anySlot.ServiceId = slot.ServiceId
                                AND anySlot.StartAt = slot.StartAt
                        )
                );
                """);

            migrationBuilder.DropIndex(
                name: "IX_AppointmentSlots_ServiceId",
                table: "AppointmentSlots");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_ServiceId_StartAt",
                table: "AppointmentSlots",
                columns: new[] { "ServiceId", "StartAt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AppointmentSlots_ServiceId_StartAt",
                table: "AppointmentSlots");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentSlots_ServiceId",
                table: "AppointmentSlots",
                column: "ServiceId");
        }
    }
}
