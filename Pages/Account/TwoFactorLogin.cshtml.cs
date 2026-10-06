using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoManager.Models;
namespace TodoManager.Pages.Account;
public class TwoFactorLoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    public TwoFactorLoginModel(SignInManager<AppUser> sm) => _signInManager = sm;
    [BindProperty, Required, StringLength(7, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(Code.Replace(" ", ""), false, false);
        if (result.Succeeded) return RedirectToPage("/Index");
        ModelState.AddModelError(string.Empty, "Неверный код.");
        return Page();
    }
}
