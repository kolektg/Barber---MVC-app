using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BarberBooking.Models;

public class AppUser : IdentityUser
{
    [StringLength(80)]
    [Display(Name = "Imie i nazwisko")]
    public string? FullName { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
