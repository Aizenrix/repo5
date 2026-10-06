using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TodoManager.Models;
namespace TodoManager.Pages.Account;
public class LogoutModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    public LogoutModel(SignInManager<AppUser> sm) => _signInManager = sm;
    public async Task<IActionResult> OnPostAsync()
    {
        await _signInManager.SignOutAsync();
        return RedirectToPage("/Index");
    }
}
