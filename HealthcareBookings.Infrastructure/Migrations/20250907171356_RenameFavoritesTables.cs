using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthcareBookings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameFavoritesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteClinics");

            migrationBuilder.DropTable(
                name: "FavoriteDoctors");

            migrationBuilder.CreateTable(
                name: "FavoriteClinic",
                columns: table => new
                {
                    PatientID = table.Column<string>(type: "TEXT", nullable: false),
                    ClinicID = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteClinic", x => new { x.PatientID, x.ClinicID });
                    table.ForeignKey(
                        name: "FK_FavoriteClinic_Clinics_ClinicID",
                        column: x => x.ClinicID,
                        principalTable: "Clinics",
                        principalColumn: "ClinicID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteClinic_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteDoctor",
                columns: table => new
                {
                    PatientID = table.Column<string>(type: "TEXT", nullable: false),
                    DoctorID = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteDoctor", x => new { x.PatientID, x.DoctorID });
                    table.ForeignKey(
                        name: "FK_FavoriteDoctor_Doctors_DoctorID",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorUID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteDoctor_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteClinic_ClinicID",
                table: "FavoriteClinic",
                column: "ClinicID");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteDoctor_DoctorID",
                table: "FavoriteDoctor",
                column: "DoctorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteClinic");

            migrationBuilder.DropTable(
                name: "FavoriteDoctor");

            migrationBuilder.CreateTable(
                name: "FavoriteClinics",
                columns: table => new
                {
                    PatientID = table.Column<string>(type: "TEXT", nullable: false),
                    ClinicID = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteClinics", x => new { x.PatientID, x.ClinicID });
                    table.ForeignKey(
                        name: "FK_FavoriteClinics_Clinics_ClinicID",
                        column: x => x.ClinicID,
                        principalTable: "Clinics",
                        principalColumn: "ClinicID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteClinics_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteDoctors",
                columns: table => new
                {
                    PatientID = table.Column<string>(type: "TEXT", nullable: false),
                    DoctorID = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteDoctors", x => new { x.PatientID, x.DoctorID });
                    table.ForeignKey(
                        name: "FK_FavoriteDoctors_Doctors_DoctorID",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorUID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteDoctors_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientUID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteClinics_ClinicID",
                table: "FavoriteClinics",
                column: "ClinicID");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteDoctors_DoctorID",
                table: "FavoriteDoctors",
                column: "DoctorID");
        }
    }
}
