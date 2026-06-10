namespace BarberBooking.Services;

public interface IAppointmentSlotGenerator
{
    Task EnsureRollingWeekAsync(CancellationToken cancellationToken = default);
}
