namespace BarberBooking.ViewModels;

public class AdminDashboardViewModel
{
    public int PendingReservations { get; set; }

    public int ApprovedReservations { get; set; }

    public int CancelledReservations { get; set; }

    public int TodaySlots { get; set; }

    public decimal ApprovedRevenue { get; set; }
}
