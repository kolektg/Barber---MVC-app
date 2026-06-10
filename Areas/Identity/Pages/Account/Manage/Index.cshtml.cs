using BarberBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace BarberBooking.Areas.Identity.Pages.Account.Manage;

[Authorize]
public class IndexModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public IndexModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public string Username { get; set; } = string.Empty;

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound("Nie znaleziono uzytkownika.");
        }

        Load(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound("Nie znaleziono uzytkownika.");
        }

        if (!ModelState.IsValid)
        {
            Load(user);
            return Page();
        }

        user.FullName = Input.FullName;
        user.PhoneNumber = Input.PhoneNumber;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            Load(user);
            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Dane konta zostaly zapisane.";
        return RedirectToPage();
    }

    private void Load(AppUser user)
    {
        Username = user.Email ?? user.UserName ?? string.Empty;
        Input = new InputModel
        {
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber
        };
    }

    public class InputModel
    {
        [StringLength(80)]
        [Display(Name = "Imie i nazwisko")]
        public string? FullName { get; set; }

        [Phone(ErrorMessage = "Podaj poprawny numer telefonu.")]
        [Display(Name = "Telefon")]
        public string? PhoneNumber { get; set; }
    }
}
