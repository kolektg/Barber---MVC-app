using BarberBooking.Data;
using BarberBooking.Models;
using BarberBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarberBooking.Controllers;

[Authorize(Roles = SeedData.AdminRole)]
public class AdminDashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var viewModel = new AdminDashboardViewModel
        {
            PendingReservations = await _context.Reservations.CountAsync(item => item.Status == ReservationStatus.Pending),
            ApprovedReservations = await _context.Reservations.CountAsync(item => item.Status == ReservationStatus.Approved),
            CancelledReservations = await _context.Reservations.CountAsync(item => item.Status == ReservationStatus.Cancelled),
            TodaySlots = await _context.AppointmentSlots.CountAsync(item => item.StartAt >= today && item.StartAt < tomorrow),
            ApprovedRevenue = await _context.Reservations
                .Where(item => item.Status == ReservationStatus.Approved)
                .Select(item => item.Service!.Price)
                .SumAsync()
        };

        return View(viewModel);
    }
}
