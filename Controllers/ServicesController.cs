using BarberBooking.Data;
using BarberBooking.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarberBooking.Controllers;

public class ServicesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAppointmentSlotGenerator _slotGenerator;

    public ServicesController(ApplicationDbContext context, IAppointmentSlotGenerator slotGenerator)
    {
        _context = context;
        _slotGenerator = slotGenerator;
    }

    public async Task<IActionResult> Index()
    {
        await _slotGenerator.EnsureRollingWeekAsync();

        var services = await _context.Services
            .Include(service => service.AppointmentSlots)
            .Where(service => service.IsActive)
            .OrderBy(service => service.Name)
            .ToListAsync();

        return View(services);
    }

    public async Task<IActionResult> Details(int id)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(item => item.Id == id && item.IsActive);

        if (service is null)
        {
            return NotFound();
        }

        return View(service);
    }

    [HttpGet]
    public async Task<IActionResult> Slots(int serviceId, DateTime? day)
    {
        await _slotGenerator.EnsureRollingWeekAsync();

        var query = _context.AppointmentSlots
            .Where(slot => slot.ServiceId == serviceId && slot.IsAvailable && slot.StartAt >= DateTime.Now);

        if (day.HasValue)
        {
            query = query.Where(slot => slot.StartAt.Date == day.Value.Date);
        }

        var rawSlots = await query
            .OrderBy(slot => slot.StartAt)
            .Select(slot => new { slot.Id, slot.StartAt })
            .ToListAsync();

        var slots = rawSlots
            .GroupBy(slot => slot.StartAt)
            .Select(group => group.OrderBy(slot => slot.Id).First())
            .Select(slot => new
            {
                slot.Id,
                Label = slot.StartAt.ToString("dd.MM.yyyy HH:mm")
            })
            .ToList();

        return Json(slots);
    }
}
