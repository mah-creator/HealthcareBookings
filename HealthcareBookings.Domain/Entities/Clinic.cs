using System.Numerics;

namespace HealthcareBookings.Domain.Entities;

public class Clinic
{
    public string ClinicID { get; set; }
    public string ClinicName { get; set; }
    public string? ClinicDescription { get; set; }
    public string ImagePath { get; set; }
    public ClinicAdmin ClinicAdmin { get; set; }
    public Location Location { get; set; }

    public List<Doctor> Doctors { get; set; }
}

public class ClinicAdmin
{
    public string ClinicAdminUID { get; set; }
    public string ClinicID { get; set; }
    public Clinic Clinic { get; set; }
}