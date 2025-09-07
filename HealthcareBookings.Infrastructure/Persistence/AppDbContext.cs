using HealthcareBookings.Domain.Entities;
using HealthcareBookings.Application.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthcareBookings.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions options) : IdentityDbContext<User>(options), IAppDbContext
{
	public DbSet<User> Users { get; set; }
	public DbSet<Patient> Patients { get; set; }
	public DbSet<ClinicAdmin> ClinicAdmins { get; set; }
	public DbSet<Clinic> Clinics { get; set; }
	public DbSet<DoctorCategory> DoctorCategories { get; set; }
	public DbSet<Doctor> Doctors { get; set; }
	public DbSet<Schedule> DoctorSchedules { get; set; }
	public DbSet<TimeSlot> DoctorTimeSlots { get; set; }
	public DbSet<Banner> Banners { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Banner>()
			.HasKey(b => b.Id);

		modelBuilder.Entity<ClinicAdmin>()
		   .HasAlternateKey(ca => ca.ClinicID);

		modelBuilder.Entity<Clinic>()
		   .HasKey(c => c.ClinicID);

		modelBuilder.Entity<Doctor>()
		   .HasKey(doc => doc.DoctorUID);

		modelBuilder.Entity<Patient>()
		   .HasKey(p => p.PatientUID);

		modelBuilder.Entity<ClinicAdmin>()
		   .HasKey(ca => ca.ClinicAdminUID);

		modelBuilder.Entity<User>()
		   .HasOne(u => u.ClinicAdminProperties)
		   .WithOne()
		   .HasForeignKey<ClinicAdmin>(ca => ca.ClinicAdminUID);
		modelBuilder.Entity<User>()
		   .HasOne(u => u.DoctorProperties)
		   .WithOne(d => d.Account)
		   .HasForeignKey<Doctor>(d => d.DoctorUID);
		modelBuilder.Entity<User>()
		   .HasOne(u => u.PatientProperties)
		   .WithOne()
		   .HasForeignKey<Patient>(p => p.PatientUID);

		// ProfileInformation entity
		modelBuilder.Entity<ProfileInformation>()
		   .HasKey(p => p.UserID);
		modelBuilder.Entity<User>()
		   .HasOne(u => u.Profile)
		   .WithOne()
		   .HasForeignKey<ProfileInformation>(p => p.UserID);

		// AppointmentReview entity
		modelBuilder.Entity<AppointmentReview>()
		   .HasKey(ar => ar.AppointmentID);

		modelBuilder.Entity<Appointment>()
		   .HasOne(a => a.Review)
		   .WithOne()
		   .HasForeignKey<AppointmentReview>(ar => ar.AppointmentID)
		   .IsRequired(true);

		// Appointment entity
		modelBuilder.Entity<Appointment>()
		   .HasKey(a => a.AppointmetnID);
		modelBuilder.Entity<Appointment>()
		   .HasAlternateKey(a => a.TimeSlotID);

		modelBuilder.Entity<Appointment>()
		   .HasOne(a => a.TimeSlot)
		   .WithOne()
		   .HasForeignKey<Appointment>(a => a.TimeSlotID);

		modelBuilder.Entity<Doctor>()
		   .HasMany(doc => doc.Appointments)
		   .WithOne(a => a.Doctor)
		   .HasForeignKey(a => a.DoctorID)
		   .IsRequired(true);

		modelBuilder.Entity<Patient>()
		   .HasMany(p => p.Appointments)
		   .WithOne(a => a.Patient)
		   .HasForeignKey(a => a.PatientID)
		   .IsRequired(true);

		// DoctorCategory entity
		modelBuilder.Entity<DoctorCategory>()
		   .HasKey(dc => dc.CategoryID);
		modelBuilder.Entity<DoctorCategory>()
			.HasAlternateKey(dc => dc.NormalizedName);
		modelBuilder.Entity<DoctorCategory>()
			.HasAlternateKey(dc => dc.CategoryName);
		modelBuilder.Entity<DoctorCategory>()
		   .HasMany(dc => dc.Doctors)
		   .WithOne(d => d.Category)
		   .HasForeignKey(d => d.CategoryID)
		   .IsRequired(true);

		// PatientLocation entity
		modelBuilder.Entity<PatientLocation>()
		   .HasKey(pl => pl.ID);
		modelBuilder.Entity<PatientLocation>()
		   .OwnsOne(pl => pl.Location);

		// Patient entity
		modelBuilder.Entity<Patient>()
		   .HasMany(p => p.Locations)
		   .WithOne()
		   .HasForeignKey(pl => pl.PatientUID);

		// FavoriteDoctors entity
		modelBuilder.Entity<FavoriteDoctor>()
		   .HasKey(fd => new { fd.PatientID, fd.DoctorID });

		modelBuilder.Entity<FavoriteDoctor>()
		   .HasOne(fd => fd.Patient)
		   .WithMany(p => p.FavoriteDoctors)
		   .HasForeignKey(fd => fd.PatientID);
		modelBuilder.Entity<FavoriteDoctor>()
		   .HasOne(fd => fd.Doctor)
		   .WithMany()
		   .HasForeignKey(fd => fd.DoctorID);

		// FavoriteClinics entity
		modelBuilder.Entity<FavoriteClinic>()
		   .HasKey(fc => new { fc.PatientID, fc.ClinicID });

		modelBuilder.Entity<FavoriteClinic>()
		   .HasOne(fc => fc.Patient)
		   .WithMany(p => p.FavoriteClinics)
		   .HasForeignKey(fc => fc.PatientID);
		modelBuilder.Entity<FavoriteClinic>()
		   .HasOne(fc => fc.Clinic)
		   .WithMany()
		   .HasForeignKey(fd => fd.ClinicID);

		// Schedule entity:
		// - primary key: ScheduleID
		// - unique constraint: (DoctorID, Date)
		modelBuilder.Entity<Schedule>()
		   .HasKey(s => s.ScheduleID);
		modelBuilder.Entity<Schedule>()
		   .HasAlternateKey(s => new { s.DoctorID, s.Date });

		// TimeSlot entity:
		// - primary key: SlotID
		// - unique constraint: (ScheduleID, Startime, EndTime)
		modelBuilder.Entity<TimeSlot>()
		   .HasKey(ts => ts.SlotID);
		modelBuilder.Entity<TimeSlot>()
		   .HasAlternateKey(ts => new { ts.ScheduleID, ts.StartTime, ts.EndTime });

		// 1-N relation between Clinic -> Doctor
		modelBuilder.Entity<Clinic>()
		   .HasMany(c => c.Doctors)
		   .WithOne(d => d.Clinic)
		   .HasForeignKey(d => d.ClinicID);

		// 1-N relation between Doctor -> Schedule
		modelBuilder.Entity<Doctor>()
		   .HasMany(d => d.Schedules)
		   .WithOne(s => s.Doctor)
		   .HasForeignKey(s => s.DoctorID);

		// 1-N relation between Schedule -> TimeSlot
		modelBuilder.Entity<Schedule>()
		   .HasMany(s => s.TimeSlots)
		   .WithOne(ts => ts.Schedule)
		   .HasForeignKey(ts => ts.ScheduleID);

		// 1-1 relation between Clinic -> ClinicAdmin
		modelBuilder.Entity<Clinic>()
		   .HasOne(c => c.ClinicAdmin)
		   .WithOne(ca => ca.Clinic)
		   .HasForeignKey<ClinicAdmin>(ca => ca.ClinicID)
		   .IsRequired(true);

		modelBuilder.Entity<Clinic>()
		   .OwnsOne(c => c.Location);
	}
}