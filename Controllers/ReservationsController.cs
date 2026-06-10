using BarberBooking.Data;
using BarberBooking.Models;
using BarberBooking.Services;
using BarberBooking.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarberBooking.Controllers;

[Authorize]
public class ReservationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IAppointmentSlotGenerator _slotGenerator;

    public ReservationsController(
        ApplicationDbContext context,
        UserManager<AppUser> userManager,
        IAppointmentSlotGenerator slotGenerator)
    {
        _context = context;
        _userManager = userManager;
        _slotGenerator = slotGenerator;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var reservations = await _context.Reservations
            .Include(reservation => reservation.Service)
            .Include(reservation => reservation.AppointmentSlot)
            .Where(reservation => reservation.UserId == userId)
            .OrderByDescending(reservation => reservation.Date)
            .ToListAsync();

        return View(reservations);
    }

    public async Task<IActionResult> Create(int serviceId)
    {
        await _slotGenerator.EnsureRollingWeekAsync();

        var user = await _userManager.GetUserAsync(User);
        var viewModel = await BuildFormViewModelAsync(serviceId);
        viewModel.CustomerName = user?.FullName ?? string.Empty;
        viewModel.PhoneNumber = user?.PhoneNumber ?? string.Empty;

        if (viewModel.Service is null)
        {
            return NotFound();
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationFormViewModel viewModel)
    {
        await _slotGenerator.EnsureRollingWeekAsync();

        var slot = await _context.AppointmentSlots
            .Include(item => item.Service)
            .FirstOrDefaultAsync(item =>
                item.Id == viewModel.AppointmentSlotId &&
                item.ServiceId == viewModel.ServiceId &&
                item.IsAvailable &&
                item.StartAt >= DateTime.Now);

        if (slot is null)
        {
            ModelState.AddModelError(nameof(viewModel.AppointmentSlotId), "Ten termin jest juz niedostepny.");
        }

        if (!ModelState.IsValid || slot is null)
        {
            await FillFormDataAsync(viewModel);
            return View(viewModel);
        }

        var reservation = new Reservation
        {
            UserId = _userManager.GetUserId(User) ?? string.Empty,
            ServiceId = viewModel.ServiceId,
            AppointmentSlotId = viewModel.AppointmentSlotId,
            Date = slot.StartAt,
            CustomerName = viewModel.CustomerName,
            PhoneNumber = viewModel.PhoneNumber,
            Notes = viewModel.Notes,
            Status = ReservationStatus.Pending
        };

        slot.IsAvailable = false;
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Rezerwacja zostala utworzona i czeka na zatwierdzenie.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var reservation = await FindOwnReservationAsync(id);

        if (reservation is null)
        {
            return NotFound();
        }

        if (reservation.Status != ReservationStatus.Pending)
        {
            return BadRequest("Mozna edytowac tylko rezerwacje oczekujace.");
        }

        var viewModel = await BuildFormViewModelAsync(reservation.ServiceId, reservation.AppointmentSlotId);
        viewModel.Id = reservation.Id;
        viewModel.AppointmentSlotId = reservation.AppointmentSlotId;
        viewModel.CustomerName = reservation.CustomerName;
        viewModel.PhoneNumber = reservation.PhoneNumber;
        viewModel.Notes = reservation.Notes;

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReservationFormViewModel viewModel)
    {
        await _slotGenerator.EnsureRollingWeekAsync();

        if (id != viewModel.Id)
        {
            return NotFound();
        }

        var reservation = await FindOwnReservationAsync(id);

        if (reservation is null)
        {
            return NotFound();
        }

        if (reservation.Status != ReservationStatus.Pending)
        {
            return BadRequest("Mozna edytowac tylko rezerwacje oczekujace.");
        }

        var selectedSlot = await _context.AppointmentSlots.FirstOrDefaultAsync(slot =>
            slot.Id == viewModel.AppointmentSlotId &&
            slot.ServiceId == reservation.ServiceId &&
            (slot.IsAvailable || slot.Id == reservation.AppointmentSlotId) &&
            slot.StartAt >= DateTime.Now);

        if (selectedSlot is null)
        {
            ModelState.AddModelError(nameof(viewModel.AppointmentSlotId), "Wybrany termin jest niedostepny.");
        }

        if (!ModelState.IsValid || selectedSlot is null)
        {
            viewModel.ServiceId = reservation.ServiceId;
            await FillFormDataAsync(viewModel, reservation.AppointmentSlotId);
            return View(viewModel);
        }

        if (reservation.AppointmentSlotId != selectedSlot.Id)
        {
            reservation.AppointmentSlot!.IsAvailable = true;
            selectedSlot.IsAvailable = false;
            reservation.AppointmentSlotId = selectedSlot.Id;
            reservation.Date = selectedSlot.StartAt;
        }

        reservation.CustomerName = viewModel.CustomerName;
        reservation.PhoneNumber = viewModel.PhoneNumber;
        reservation.Notes = viewModel.Notes;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Rezerwacja zostala zaktualizowana.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var reservation = await FindOwnReservationAsync(id);

        if (reservation is null)
        {
            return NotFound();
        }

        if (reservation.Status is ReservationStatus.Pending or ReservationStatus.Approved)
        {
            reservation.Status = ReservationStatus.Cancelled;
            reservation.AppointmentSlot!.IsAvailable = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Rezerwacja zostala anulowana.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<Reservation?> FindOwnReservationAsync(int id)
    {
        var userId = _userManager.GetUserId(User);
        return await _context.Reservations
            .Include(reservation => reservation.Service)
            .Include(reservation => reservation.AppointmentSlot)
            .FirstOrDefaultAsync(reservation => reservation.Id == id && reservation.UserId == userId);
    }

    private async Task<ReservationFormViewModel> BuildFormViewModelAsync(int serviceId, int? selectedSlotId = null)
    {
        var viewModel = new ReservationFormViewModel
        {
            ServiceId = serviceId,
            Service = await _context.Services.FirstOrDefaultAsync(service => service.Id == serviceId)
        };

        await FillFormDataAsync(viewModel, selectedSlotId);
        return viewModel;
    }

    private async Task FillFormDataAsync(ReservationFormViewModel viewModel, int? selectedSlotId = null)
    {
        await _slotGenerator.EnsureRollingWeekAsync();

        viewModel.Service = await _context.Services.FirstOrDefaultAsync(service => service.Id == viewModel.ServiceId);
        var slots = await _context.AppointmentSlots
            .Where(slot =>
                slot.ServiceId == viewModel.ServiceId &&
                slot.StartAt >= DateTime.Now &&
                (slot.IsAvailable || slot.Id == selectedSlotId))
            .OrderBy(slot => slot.StartAt)
            .ToListAsync();

        viewModel.AvailableSlots = slots
            .GroupBy(slot => slot.StartAt)
            .Select(group =>
                group.FirstOrDefault(slot => slot.Id == selectedSlotId) ??
                group.OrderBy(slot => slot.Id).First())
            .ToList();
    }
}
