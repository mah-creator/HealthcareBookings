using System.Drawing;

namespace HealthcareBookings.Domain.Entities;

public class Banner
{
	public string Id { get; set; } = Guid.NewGuid().ToString();
	public string ImagePath { get; set; }
}
