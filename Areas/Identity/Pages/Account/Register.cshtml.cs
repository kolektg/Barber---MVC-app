using BarberBooking.Data;
using BarberBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace BarberBooking.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public RegisterModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string ReturnUrl { get; set; } = "/";

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");

        if (ModelState.IsValid)
        {
            var user = new AppUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true,
                FullName = Input.FullName,
                PhoneNumber = Input.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, SeedData.UserRole);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(ReturnUrl);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return Page();
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Podaj imie i nazwisko.")]
        [StringLength(80)]
        [Display(Name = "Imie i nazwisko")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Podaj adres e-mail.")]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Podaj numer telefonu.")]
        [Phone]
        [Display(Name = "Telefon")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Podaj haslo.")]
        [StringLength(100, ErrorMessage = "{0} musi miec co najmniej {2} znakow.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Haslo")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Powtorz haslo")]
        [Compare(nameof(Password), ErrorMessage = "Hasla nie sa takie same.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
