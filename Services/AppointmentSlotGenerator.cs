using BarberBooking.Data;
using BarberBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace BarberBooking.Services;

public class AppointmentSlotGenerator : IAppointmentSlotGenerator
{
    private static readonly TimeSpan SlotStep = TimeSpan.FromMinutes(30);
    private const int DaysAhead = 7;

    private readonly ApplicationDbContext _context;

    public AppointmentSlotGenerator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task EnsureRollingWeekAsync(CancellationToken cancellationToken = default)
    {
        var services = await _context.Services
            .Where(service => service.IsActive)
            .ToListAsync(cancellationToken);

        if (services.Count == 0)
        {
            return;
        }

        var today = DateTime.Today;
        var horizon = today.AddDays(DaysAhead);
        var now = DateTime.Now;

        var serviceIds = services.Select(service => service.Id).ToList();
        var existingSlots = await _context.AppointmentSlots
            .Where(slot =>
                serviceIds.Contains(slot.ServiceId) &&
                slot.StartAt >= today &&
                slot.StartAt < horizon)
            .Select(slot => new { slot.ServiceId, slot.StartAt })
            .ToListAsync(cancellationToken);

        var existingKeys = existingSlots
            .Select(slot => BuildKey(slot.ServiceId, slot.StartAt))
            .ToHashSet();

        var slotsToAdd = new List<AppointmentSlot>();

        for (var dayOffset = 0; dayOffset < DaysAhead; dayOffset++)
        {
            var day = today.AddDays(dayOffset);
            var (opensAt, closesAt) = GetOpeningHours(day);

            foreach (var service in services)
            {
                var startAt = day.Add(opensAt);
                var latestEnd = day.Add(closesAt);

                while (startAt.AddMinutes(service.DurationMinutes) <= latestEnd)
                {
                    if (startAt > now && existingKeys.Add(BuildKey(service.Id, startAt)))
                    {
                        slotsToAdd.Add(new AppointmentSlot
                        {
                            ServiceId = service.Id,
                            StartAt = startAt,
                            EndAt = startAt.AddMinutes(service.DurationMinutes),
                            IsAvailable = true
                        });
                    }

                    startAt = startAt.Add(SlotStep);
                }
            }
        }

        if (slotsToAdd.Count > 0)
        {
            _context.AppointmentSlots.AddRange(slotsToAdd);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                foreach (var slot in slotsToAdd)
                {
                    _context.Entry(slot).State = EntityState.Detached;
                }
            }
        }
    }

    private static (TimeSpan OpensAt, TimeSpan ClosesAt) GetOpeningHours(DateTime day)
    {
        return day.DayOfWeek == DayOfWeek.Sunday
            ? (TimeSpan.FromHours(10), TimeSpan.FromHours(20))
            : (TimeSpan.FromHours(9), TimeSpan.FromHours(21));
    }

    private static string BuildKey(int serviceId, DateTime startAt)
    {
        return $"{serviceId}:{startAt:O}";
    }
}
