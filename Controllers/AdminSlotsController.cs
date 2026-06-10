using BarberBooking.Data;
using BarberBooking.Models;
using BarberBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BarberBooking.Controllers;

[Authorize(Roles = SeedData.AdminRole)]
public class AdminSlotsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppointmentSlotGenerator _slotGenerator;

    public AdminSlotsController(ApplicationDbContext context, IAppointmentSlotGenerator slotGenerator)
    {
        _context = context;
        _slotGenerator = slotGenerator;
    }

    public async Task<IActionResult> Index(DateTime? day, int? serviceId)
    {
        await _slotGenerator.EnsureRollingWeekAsync();

        var selectedDay = (day ?? DateTime.Today).Date;
        var nextDay = selectedDay.AddDays(1);

        var query = _context.AppointmentSlots
            .Include(slot => slot.Service)
            .Where(slot => slot.StartAt >= selectedDay && slot.StartAt < nextDay);

        if (serviceId.HasValue)
        {
            query = query.Where(slot => slot.ServiceId == serviceId.Value);
        }

        var slots = await query
            .OrderBy(slot => slot.StartAt)
            .ThenBy(slot => slot.Service!.Name)
            .ToListAsync();

        ViewBag.SelectedDay = selectedDay.ToString("yyyy-MM-dd");
        ViewBag.SelectedServiceId = serviceId;
        await LoadServicesAsync(serviceId);

        return View(slots);
    }

    public async Task<IActionResult> Create()
    {
        await LoadServicesAsync();
        return View(new AppointmentSlot { StartAt = DateTime.Today.AddDays(1).AddHours(9) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentSlot slot)
    {
        var service = await _context.Services.FindAsync(slot.ServiceId);
        if (service is null)
        {
            ModelState.AddModelError(nameof(slot.ServiceId), "Wybierz usluge.");
        }
        else
        {
            slot.EndAt = slot.StartAt.AddMinutes(service.DurationMinutes);
        }

        if (!ModelState.IsValid)
        {
            await LoadServicesAsync();
            return View(slot);
        }

        _context.AppointmentSlots.Add(slot);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var slot = await _context.AppointmentSlots.FindAsync(id);

        if (slot is null)
        {
            return NotFound();
        }

        await LoadServicesAsync();
        return View(slot);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AppointmentSlot slot)
    {
        if (id != slot.Id)
        {
            return NotFound();
        }

        var service = await _context.Services.FindAsync(slot.ServiceId);
        if (service is null)
        {
            ModelState.AddModelError(nameof(slot.ServiceId), "Wybierz usluge.");
        }
        else
        {
            slot.EndAt = slot.StartAt.AddMinutes(service.DurationMinutes);
        }

        if (!ModelState.IsValid)
        {
            await LoadServicesAsync();
            return View(slot);
        }

        _context.Update(slot);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var slot = await _context.AppointmentSlots
            .Include(item => item.Service)
            .FirstOrDefaultAsync(item => item.Id == id);

        return slot is null ? NotFound() : View(slot);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var slot = await _context.AppointmentSlots.FindAsync(id);

        if (slot is not null)
        {
            _context.AppointmentSlots.Remove(slot);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadServicesAsync(int? selectedServiceId = null)
    {
        var services = await _context.Services.OrderBy(item => item.Name).ToListAsync();
        ViewBag.Services = new SelectList(services, "Id", "Name", selectedServiceId);
    }
}
