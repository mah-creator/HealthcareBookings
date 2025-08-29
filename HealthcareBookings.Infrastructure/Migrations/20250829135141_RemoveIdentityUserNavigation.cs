using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthcareBookings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIdentityUserNavigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClinicAdmins_AspNetUsers_AdminUserId",
                table: "ClinicAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_AspNetUsers_DoctorUserId",
                table: "Doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_Patients_AspNetUsers_PatientUserId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Patients_PatientUserId",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_DoctorUserId",
                table: "Doctors");

            migrationBuilder.DropIndex(
                name: "IX_ClinicAdmins_AdminUserId",
                table: "ClinicAdmins");

            migrationBuilder.DropColumn(
                name: "PatientUserId",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "DoctorUserId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "AdminUserId",
                table: "ClinicAdmins");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PatientUserId",
                table: "Patients",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorUserId",
                table: "Doctors",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdminUserId",
                table: "ClinicAdmins",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PatientUserId",
                table: "Patients",
                column: "PatientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_DoctorUserId",
                table: "Doctors",
                column: "DoctorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicAdmins_AdminUserId",
                table: "ClinicAdmins",
                column: "AdminUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicAdmins_AspNetUsers_AdminUserId",
                table: "ClinicAdmins",
                column: "AdminUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_AspNetUsers_DoctorUserId",
                table: "Doctors",
                column: "DoctorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Patients_AspNetUsers_PatientUserId",
                table: "Patients",
                column: "PatientUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
