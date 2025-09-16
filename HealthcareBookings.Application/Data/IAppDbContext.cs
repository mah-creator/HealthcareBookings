using HealthcareBookings.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBookings.Application.Data;

public interface IAppDbContext
{
	public DbSet<User> Users { get; set; }
	public DbSet<Patient> Patients { get; set; }
	public DbSet<ClinicAdmin> ClinicAdmins { get; set; }
	public DbSet<Clinic> Clinics { get; set; }
	public DbSet<DoctorCategory> DoctorCategories { get; set; }
	public DbSet<Doctor> Doctors { get; set; }
	public DbSet<Schedule> DoctorSchedules { get; set; }
	public DbSet<TimeSlot> DoctorTimeSlots { get; set; }
	public DbSet<Appointment> Appointments { get; set; }
	public DbSet<Banner> Banners { get; set; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
