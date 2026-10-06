using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoManager.Models;
namespace TodoManager.Pages.Account;
public class LoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    public LoginModel(SignInManager<AppUser> sm) => _signInManager = sm;
    [BindProperty]
    public InputModel Input { get; set; } = new();
    public class InputModel
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded) return RedirectToPage("/Index");
        if (result.RequiresTwoFactor) return RedirectToPage("/Account/TwoFactorLogin");
        if (result.IsLockedOut) ModelState.AddModelError(string.Empty, "Аккаунт заблокирован на 15 минут.");
        else ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
        return Page();
    }
}
