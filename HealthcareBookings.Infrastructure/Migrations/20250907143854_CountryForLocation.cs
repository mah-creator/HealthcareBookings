using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthcareBookings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CountryForLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location_Country",
                table: "PatientLocation",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location_Country",
                table: "Clinics",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location_Country",
                table: "PatientLocation");

            migrationBuilder.DropColumn(
                name: "Location_Country",
                table: "Clinics");
        }
    }
}
