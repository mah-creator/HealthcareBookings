using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthcareBookings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameClinicEntityToClinics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClinicAdmins_Clinic_ClinicID",
                table: "ClinicAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Clinic_ClinicID",
                table: "Doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteClinics_Clinic_ClinicID",
                table: "FavoriteClinics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clinic",
                table: "Clinic");

            migrationBuilder.RenameTable(
                name: "Clinic",
                newName: "Clinics");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clinics",
                table: "Clinics",
                column: "ClinicID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicAdmins_Clinics_ClinicID",
                table: "ClinicAdmins",
                column: "ClinicID",
                principalTable: "Clinics",
                principalColumn: "ClinicID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Clinics_ClinicID",
                table: "Doctors",
                column: "ClinicID",
                principalTable: "Clinics",
                principalColumn: "ClinicID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteClinics_Clinics_ClinicID",
                table: "FavoriteClinics",
                column: "ClinicID",
                principalTable: "Clinics",
                principalColumn: "ClinicID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClinicAdmins_Clinics_ClinicID",
                table: "ClinicAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_Clinics_ClinicID",
                table: "Doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteClinics_Clinics_ClinicID",
                table: "FavoriteClinics");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clinics",
                table: "Clinics");

            migrationBuilder.RenameTable(
                name: "Clinics",
                newName: "Clinic");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clinic",
                table: "Clinic",
                column: "ClinicID");

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicAdmins_Clinic_ClinicID",
                table: "ClinicAdmins",
                column: "ClinicID",
                principalTable: "Clinic",
                principalColumn: "ClinicID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_Clinic_ClinicID",
                table: "Doctors",
                column: "ClinicID",
                principalTable: "Clinic",
                principalColumn: "ClinicID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteClinics_Clinic_ClinicID",
                table: "FavoriteClinics",
                column: "ClinicID",
                principalTable: "Clinic",
                principalColumn: "ClinicID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
