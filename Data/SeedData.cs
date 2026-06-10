using BarberBooking.Models;
using BarberBooking.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BarberBooking.Data;

public static class SeedData
{
    public const string AdminRole = "Admin";
    public const string UserRole = "User";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();

        await EnsureRoleAsync(roleManager, AdminRole);
        await EnsureRoleAsync(roleManager, UserRole);

        await EnsureUserAsync(userManager, "admin@barberbooking.local", "Admin123!", "Administrator", AdminRole);
        await EnsureUserAsync(userManager, "user@barberbooking.local", "User123!", "Jan Klient", UserRole);

        if (!await context.Services.AnyAsync())
        {
            var servicesSeed = new List<Service>
            {
                new()
                {
                    Name = "Strzyzenie meskie",
                    Description = "Klasyczne strzyzenie z konsultacja, myciem i stylizacja.",
                    Price = 70,
                    DurationMinutes = 45
                },
                new()
                {
                    Name = "Strzyzenie brody",
                    Description = "Modelowanie brody, konturowanie brzytwa i pielegnacja.",
                    Price = 45,
                    DurationMinutes = 30
                },
                new()
                {
                    Name = "Combo: wlosy + broda",
                    Description = "Pelna wizyta barberska z doborem fryzury i ksztaltu brody.",
                    Price = 110,
                    DurationMinutes = 75
                },
                new()
                {
                    Name = "Koloryzacja i tonowanie",
                    Description = "Koloryzacja, odswiezenie tonu lub subtelna zmiana odcienia.",
                    Price = 180,
                    DurationMinutes = 120
                }
            };

            context.Services.AddRange(servicesSeed);
            await context.SaveChangesAsync();

        }

        var slotGenerator = services.GetRequiredService<IAppointmentSlotGenerator>();
        await slotGenerator.EnsureRollingWeekAsync();
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    private static async Task EnsureUserAsync(
        UserManager<AppUser> userManager,
        string email,
        string password,
        string fullName,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            user = new AppUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName
            };

            await userManager.CreateAsync(user, password);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }
}
