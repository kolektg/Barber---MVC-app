using System.ComponentModel.DataAnnotations;

namespace BarberBooking.Models;

public class AppointmentSlot
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Start")]
    public DateTime StartAt { get; set; }

    [Display(Name = "Koniec")]
    public DateTime EndAt { get; set; }

    [Display(Name = "Dostepny")]
    public bool IsAvailable { get; set; } = true;

    [Display(Name = "Usluga")]
    public int ServiceId { get; set; }

    public Service? Service { get; set; }

    public Reservation? Reservation { get; set; }
}
