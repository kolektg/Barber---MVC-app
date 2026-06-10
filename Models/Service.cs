using System.ComponentModel.DataAnnotations;

namespace BarberBooking.Models;

public class Service
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nazwa uslugi jest wymagana.")]
    [StringLength(80)]
    [Display(Name = "Nazwa")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Opis jest wymagany.")]
    [StringLength(600)]
    [Display(Name = "Opis")]
    public string Description { get; set; } = string.Empty;

    [Range(1, 2000, ErrorMessage = "Cena musi byc wieksza od 0.")]
    [DataType(DataType.Currency)]
    [Display(Name = "Cena")]
    public decimal Price { get; set; }

    [Range(15, 300, ErrorMessage = "Czas trwania musi miescic sie w zakresie 15-300 minut.")]
    [Display(Name = "Czas trwania (min)")]
    public int DurationMinutes { get; set; } = 60;

    [Display(Name = "Aktywna")]
    public bool IsActive { get; set; } = true;

    public ICollection<AppointmentSlot> AppointmentSlots { get; set; } = new List<AppointmentSlot>();

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
