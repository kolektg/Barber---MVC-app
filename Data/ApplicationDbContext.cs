using BarberBooking.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BarberBooking.Data;

public class ApplicationDbContext : IdentityDbContext<AppUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Service> Services => Set<Service>();

    public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();

    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Service>()
            .Property(service => service.Price)
            .HasPrecision(10, 2);

        builder.Entity<Service>()
            .HasMany(service => service.AppointmentSlots)
            .WithOne(slot => slot.Service)
            .HasForeignKey(slot => slot.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Service>()
            .HasMany(service => service.Reservations)
            .WithOne(reservation => reservation.Service)
            .HasForeignKey(reservation => reservation.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AppUser>()
            .HasMany(user => user.Reservations)
            .WithOne(reservation => reservation.User)
            .HasForeignKey(reservation => reservation.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AppointmentSlot>()
            .HasIndex(slot => new { slot.ServiceId, slot.StartAt })
            .IsUnique();

        builder.Entity<AppointmentSlot>()
            .HasOne(slot => slot.Reservation)
            .WithOne(reservation => reservation.AppointmentSlot)
            .HasForeignKey<Reservation>(reservation => reservation.AppointmentSlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
