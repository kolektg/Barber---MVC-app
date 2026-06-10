using BarberBooking.Models;
using System.ComponentModel.DataAnnotations;

namespace BarberBooking.ViewModels;

public class ReservationFormViewModel
{
    public int? Id { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [Required(ErrorMessage = "Wybierz termin wizyty.")]
    [Display(Name = "Termin")]
    public int AppointmentSlotId { get; set; }

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

    public Service? Service { get; set; }

    public List<AppointmentSlot> AvailableSlots { get; set; } = new();
}
