using BarberBooking.Data;
using BarberBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarberBooking.Controllers;

[Authorize(Roles = SeedData.AdminRole)]
public class AdminServicesController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminServicesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Services.OrderBy(service => service.Name).ToListAsync());
    }

    public IActionResult Create()
    {
        return View(new Service());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Service service)
    {
        if (!ModelState.IsValid)
        {
            return View(service);
        }

        _context.Services.Add(service);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var service = await _context.Services.FindAsync(id);
        return service is null ? NotFound() : View(service);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Service service)
    {
        if (id != service.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(service);
        }

        _context.Update(service);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var service = await _context.Services.FirstOrDefaultAsync(item => item.Id == id);
        return service is null ? NotFound() : View(service);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var service = await _context.Services.FindAsync(id);

        if (service is not null)
        {
            var hasReservations = await _context.Reservations.AnyAsync(item => item.ServiceId == id);

            if (hasReservations)
            {
                service.IsActive = false;
                TempData["Info"] = "Usluga ma rezerwacje, wiec zostala oznaczona jako nieaktywna.";
            }
            else
            {
                _context.Services.Remove(service);
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
