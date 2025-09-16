using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthcareBookings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameAppointmentsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_DoctorTimeSlots_TimeSlotID",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Doctors_DoctorID",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Patients_PatientID",
                table: "Appointment");

            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentReview_Appointment_AppointmentID",
                table: "AppointmentReview");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Appointment_TimeSlotID",
                table: "Appointment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Appointment",
                table: "Appointment");

            migrationBuilder.RenameTable(
                name: "Appointment",
                newName: "Appointments");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_PatientID",
                table: "Appointments",
                newName: "IX_Appointments_PatientID");

            migrationBuilder.RenameIndex(
                name: "IX_Appointment_DoctorID",
                table: "Appointments",
                newName: "IX_Appointments_DoctorID");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Appointments_TimeSlotID",
                table: "Appointments",
                column: "TimeSlotID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Appointments",
                table: "Appointments",
                column: "AppointmetnID");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentReview_Appointments_AppointmentID",
                table: "AppointmentReview",
                column: "AppointmentID",
                principalTable: "Appointments",
                principalColumn: "AppointmetnID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_DoctorTimeSlots_TimeSlotID",
                table: "Appointments",
                column: "TimeSlotID",
                principalTable: "DoctorTimeSlots",
                principalColumn: "SlotID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Doctors_DoctorID",
                table: "Appointments",
                column: "DoctorID",
                principalTable: "Doctors",
                principalColumn: "DoctorUID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Patients_PatientID",
                table: "Appointments",
                column: "PatientID",
                principalTable: "Patients",
                principalColumn: "PatientUID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentReview_Appointments_AppointmentID",
                table: "AppointmentReview");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_DoctorTimeSlots_TimeSlotID",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Doctors_DoctorID",
                table: "Appointments");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Patients_PatientID",
                table: "Appointments");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Appointments_TimeSlotID",
                table: "Appointments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Appointments",
                table: "Appointments");

            migrationBuilder.RenameTable(
                name: "Appointments",
                newName: "Appointment");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_PatientID",
                table: "Appointment",
                newName: "IX_Appointment_PatientID");

            migrationBuilder.RenameIndex(
                name: "IX_Appointments_DoctorID",
                table: "Appointment",
                newName: "IX_Appointment_DoctorID");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Appointment_TimeSlotID",
                table: "Appointment",
                column: "TimeSlotID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Appointment",
                table: "Appointment",
                column: "AppointmetnID");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_DoctorTimeSlots_TimeSlotID",
                table: "Appointment",
                column: "TimeSlotID",
                principalTable: "DoctorTimeSlots",
                principalColumn: "SlotID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Doctors_DoctorID",
                table: "Appointment",
                column: "DoctorID",
                principalTable: "Doctors",
                principalColumn: "DoctorUID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Patients_PatientID",
                table: "Appointment",
                column: "PatientID",
                principalTable: "Patients",
                principalColumn: "PatientUID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentReview_Appointment_AppointmentID",
                table: "AppointmentReview",
                column: "AppointmentID",
                principalTable: "Appointment",
                principalColumn: "AppointmetnID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
