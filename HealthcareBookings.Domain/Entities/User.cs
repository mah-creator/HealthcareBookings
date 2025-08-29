using Microsoft.AspNetCore.Identity;

namespace HealthcareBookings.Domain.Entities;

public class User : IdentityUser
{
    public ProfileInformation Profile;

    // ClinicAdmin-specifc properties
    public ClinicAdmin ClinicAdminProperties { get; set; }

    // Patient-specific properties
    public Patient PatientProperties { get; set; }

    // Doctor-specific properties
    public Doctor DoctorProperties { get; set; }
}

public class ProfileInformation
{
    public string UserID { get; set; }
    public string Name { get; set; }
    public string Gender { get; set; }
    public DateOnly DOB { get; set; }
    public string ProfileImagePath { get; set; }
}