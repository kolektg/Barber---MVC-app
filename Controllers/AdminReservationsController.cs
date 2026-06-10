using BarberBooking.Data;
using BarberBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarberBooking.Controllers;

[Authorize(Roles = SeedData.AdminRole)]
public class AdminReservationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminReservationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var reservations = await _context.Reservations
            .Include(reservation => reservation.Service)
            .Include(reservation => reservation.AppointmentSlot)
            .Include(reservation => reservation.User)
            .OrderByDescending(reservation => reservation.CreatedAt)
            .ToListAsync();

        return View(reservations);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);

        if (reservation is null)
        {
            return NotFound();
        }

        reservation.Status = ReservationStatus.Approved;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var reservation = await _context.Reservations
            .Include(item => item.AppointmentSlot)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (reservation is null)
        {
            return NotFound();
        }

        reservation.Status = ReservationStatus.Rejected;
        reservation.AppointmentSlot!.IsAvailable = true;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
