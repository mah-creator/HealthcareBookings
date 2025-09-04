using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HealthcareBookings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizedCategoryName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "DoctorCategories",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_DoctorCategories_CategoryName",
                table: "DoctorCategories",
                column: "CategoryName");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_DoctorCategories_NormalizedName",
                table: "DoctorCategories",
                column: "NormalizedName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_DoctorCategories_CategoryName",
                table: "DoctorCategories");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_DoctorCategories_NormalizedName",
                table: "DoctorCategories");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "DoctorCategories");
        }
    }
}
