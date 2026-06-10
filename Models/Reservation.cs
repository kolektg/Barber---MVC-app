using System.ComponentModel.DataAnnotations;

namespace BarberBooking.Models;

public class Reservation
{
    public int Id { get; set; }

    [Display(Name = "Data wizyty")]
    public DateTime Date { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public AppUser? User { get; set; }

    [Display(Name = "Usluga")]
    public int ServiceId { get; set; }

    public Service? Service { get; set; }

    [Display(Name = "Termin")]
    public int AppointmentSlotId { get; set; }

    public AppointmentSlot? AppointmentSlot { get; set; }

    [Required(ErrorMessage = "Podaj imie i nazwisko.")]
    [StringLength(80)]
    [Display(Name = "Imie i nazwisko")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Podaj numer telefonu.")]
    [Phone(ErrorMessage = "Podaj poprawny numer telefonu.")]
    [StringLength(30)]
    [Display(Name = "Telefon")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Uwagi")]
    public string? Notes { get; set; }

    [Display(Name = "Status")]
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    [Display(Name = "Utworzono")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
