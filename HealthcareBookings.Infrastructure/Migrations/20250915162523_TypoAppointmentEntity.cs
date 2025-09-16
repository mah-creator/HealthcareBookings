using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthcareBookings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TypoAppointmentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AppointmetnID",
                table: "Appointments",
                newName: "AppointmentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AppointmentID",
                table: "Appointments",
                newName: "AppointmetnID");
        }
    }
}
