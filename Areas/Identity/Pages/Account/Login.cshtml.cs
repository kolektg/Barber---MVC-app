using BarberBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace BarberBooking.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;

    public LoginModel(SignInManager<AppUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string ReturnUrl { get; set; } = "/";

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ReturnUrl = string.IsNullOrWhiteSpace(ReturnUrl) ? Url.Content("~/") : ReturnUrl;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            Input.Email,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            return LocalRedirect(ReturnUrl);
        }

        ModelState.AddModelError(string.Empty, "Nieprawidlowy e-mail lub haslo.");
        return Page();
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Podaj adres e-mail.")]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Podaj haslo.")]
        [DataType(DataType.Password)]
        [Display(Name = "Haslo")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Zapamietaj mnie")]
        public bool RememberMe { get; set; }
    }
}
